using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CityGame.Classes.Rendering.Particles;
using Microsoft.Xna.Framework.Graphics;
using OrpticonGameHelper;
using OrpticonGameHelper.Classes;
using OrpticonGameHelper.Classes.Utils;

namespace CityGame.Classes.Rendering.Shaders;

public sealed class ShaderLoader : ILoadable
{
    private static string[] _shaderPaths = new[]
    {
        Explosion.SHADER_PATH
    };

    private static ShaderLoader _instance;

    private Dictionary<string, Effect> _effects = new Dictionary<string, Effect>();

    private ShaderLoader() {}

    public static ShaderLoader Create()
    {
        if (ShaderLoader._instance != null)
        {
            throw new Exception("There can only be one ShaderLoader.");
        }
        
        ShaderLoader._instance = new ShaderLoader();
        return ShaderLoader._instance;
    }
    
    public void Load(Window window)
    {
        
        foreach (string shaderPath in ShaderLoader._shaderPaths)
        {
            byte[] shaderCode = AssemblyUtility.ReadAssemblyFileAsByte(shaderPath);
        
            Effect shader = new Effect(window.GraphicsDevice, shaderCode);
            this._effects[shaderPath] = shader;
        }
    }

    public static Effect Get(string path)
    {
        if (!ShaderLoader._instance._effects.ContainsKey(path))
        {
            throw new Exception("No effect loaded with the path '" + path + "'.");
        }
        
        return ShaderLoader._instance._effects[path];
    }
}