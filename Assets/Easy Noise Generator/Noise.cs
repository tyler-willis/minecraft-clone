using UnityEngine;
using System.Collections.Generic;
using System;
public enum NoiseStyle //Available styles
{
	Linear,
	Second,
	Third,
	Fourth
};
public enum Edge //Edge styles
{
	Smooth,
	Sharp
};
public enum Algorithm //Available algorithms
{
	Perlin3d,
	Simplex2d,
	Simplex3d,
	Marble3d,
	Turbulence3d,
	Wood3d
};
public class FractalSettings //Settings for Fractal noise
{
	public int layers;
	public float lacunarity;
	public float gain;
}
public class ErrorOutput : MonoBehaviour
{
	public static void error(object message)
	{
		Debug.LogError (message);
		Debug.Break();
	}
}
public class Noise// Main noise function
{
    private readonly System.Random _random;
	private List<Channel> channel = new List<Channel>();
	PerlinAlgorithm perlin;
	SimplexAlgorithm simplex;
    public Noise()
    {
		//Initialize the main noise function using a random seed
        _random = new System.Random();
		perlin = new PerlinAlgorithm(_random);
		simplex = new SimplexAlgorithm(_random);
    }
    public Noise(int seed)
    {
		//Initialize the main noise function using a custom seed
        _random = new System.Random(seed);
		perlin = new PerlinAlgorithm(_random);
		simplex = new SimplexAlgorithm(_random);
    }
	public void addChannel(Channel c)
	{
		//Add a channel to the Noise generator. Each channel can have a different algorithm and settings
		channel.Add(c);
	}
	public float getNoise(Vector3 p,String channelName)
	{
		//Get the noise value at a certain point on a certain channel
		int channelID = -1;
		for(int n=0;n<channel.Count;n++)//Convert channelName to channelID
		{
			if(channel[n].name==channelName)
				channelID = n;
		}
		if(channelID==-1)
		{
			ErrorOutput.error ("The channel: \""+channelName+"\" could not be found.");
			return 0.0f;
		}
		
		float result = getRawNoise(p,channelID);//Calculate the noise value
		
		if(channel[channelID].edge==Edge.Sharp)//If the edge is sharp check whether or not the value is above the threshold
		{
			if(result>channel[channelID].edge_threshold)
				return channel[channelID].max;
			else
				return 0.0f;
		}
		return result;
	}
	public float getRawNoise(Vector3 inputVector,int channelID)
	{					
		//Returns the raw noise value at a certain point using the correct algorith
		inputVector += new Vector3(10000.0f*(float)channelID,10000.0f*(float)channelID,10000.0f*(float)channelID);
		float result = 0.0f;
		float frequency = 1.0f;
		float amplitude = 1.0f;
		float maxsum = 0.0f;
		for(int f=0;f<channel[channelID].fractalsettings.layers;f++)//Repeat the calculation for Fractal layers (once if Fractal is disabled)
		{
			maxsum += amplitude;
			if(channel[channelID].algorithm==Algorithm.Perlin3d)//Perlin3d noise function.
			{
				Vector3 outputVector = inputVector/channel[channelID].scale;
				outputVector*=frequency;
				result += amplitude*perlin.noise3D(outputVector);
			}
			if(channel[channelID].algorithm==Algorithm.Simplex2d)//Simplex2d noise function. 
			{
				Vector3 outputVector = inputVector/(1.7f*channel[channelID].scale);
				outputVector*=frequency;
				result += amplitude*simplex.noise2D(outputVector);
			}
			if(channel[channelID].algorithm==Algorithm.Simplex3d)//Simplex3d noise function.
			{
				Vector3 outputVector = inputVector/(1.5f*channel[channelID].scale);
				outputVector*=frequency;
				result += amplitude*simplex.noise3D(outputVector);
			}
			if(channel[channelID].algorithm==Algorithm.Marble3d)//Marble3d noise function.
			{
				Vector3 outputVector = inputVector/(1.5f*channel[channelID].scale);
				outputVector*=frequency;
				result += amplitude*(0.5f+0.5f*(float)Math.Sin((double)(outputVector.x*2.0f+2.0f*simplex.noise3D(outputVector))));
			}
			if(channel[channelID].algorithm==Algorithm.Turbulence3d)//Turbulence3d noise function.
			{
				Vector3 outputVector = inputVector/(1.5f*channel[channelID].scale);
				outputVector*=frequency;
				result += Mathf.Abs(amplitude*(-1.0f+2.0f*perlin.noise3D(outputVector)));
			}
			if(channel[channelID].algorithm==Algorithm.Wood3d)//Wood3d noise function.
			{
				Vector3 outputVector = inputVector/(1.4f*channel[channelID].scale);
				outputVector*=frequency;
				float n = simplex.noise3D(outputVector);
				float g = n * 10.0f;
				n = g - (float)Math.Round((double)g);
				result +=amplitude*n;
			}
			
			//Adjust the frequency and amplitude of the next Fractal layer
			frequency *= channel[channelID].fractalsettings.lacunarity;
			amplitude *= channel[channelID].fractalsettings.gain;
		}
		result /= maxsum;
		if(channel[channelID].style == NoiseStyle.Second)//Raise the value to the 2nd power if enabled
			result *= result;
		if(channel[channelID].style == NoiseStyle.Third)//Raise the value to the 3rd power if enabled
			result *= result*result;
		if(channel[channelID].style == NoiseStyle.Fourth)//Raise the value to the 4th power if enabled
			result *= result*result*result;
		
		//Adjust the range of the values from [0...1] to [min...max]
		return channel[channelID].min+(channel[channelID].max-channel[channelID].min)*result;
	}
}

//The channel class. A channel contains specific value and algorithm settings 
public class Channel
{
	public String name;
	public float scale;
	public float min;
	public float max;
	public float edge_threshold;
	public NoiseStyle style;
	public Edge edge;
	public Algorithm algorithm;
	public bool fractalEnabled;
	public FractalSettings fractalsettings;
	public Channel setFractal(int layers,float lacunarity,float gain)//Used to enable Fractal noise.
	{		
		fractalEnabled = true;
		fractalsettings.layers = layers;
		fractalsettings.lacunarity = lacunarity;
		fractalsettings.gain = gain;
		return this;
	}
	public Channel(String name,Algorithm algorithm)
	{		
		init (name,algorithm,10.0f,NoiseStyle.Linear,0.0f,1.0f,Edge.Smooth,0.5f);
	}
	public Channel(String name,Algorithm algorithm,float scale,NoiseStyle style,float min,float max,Edge edge,float edge_threshold)
	{
		init (name,algorithm,scale,style,min,max,edge,edge_threshold);
	}
	public Channel(String name,Algorithm algorithm,float scale,NoiseStyle style,float min,float max,Edge edge)
	{		
		init (name,algorithm,scale,style,min,max,edge,max*0.5f);
	}
	void init(String p_name,Algorithm p_algorithm,float p_scale,NoiseStyle p_style,float p_min,float p_max,Edge p_edge,float p_edge_threshold)
	{
		fractalEnabled = false;
		fractalsettings = new FractalSettings();
		fractalsettings.layers = 1;
		fractalsettings.lacunarity = 1.0f;
		fractalsettings.gain = 1.0f;
		
		name = p_name;
		algorithm = p_algorithm;
		scale = p_scale;
		min = p_min;
		max = p_max;
		style = p_style;
		edge = p_edge;
		edge_threshold = p_edge_threshold;		
	}
}
