/*
namespace CityGame
{
    public class CustomShader : ShaderEffect
    {
        private const string _kshaderAsBase64 = @"AAP///7/HwBDVEFCHAAAAE8AAAAAA///AQAAABwAAAAAAQAASAAAADAAAAADAAAAAQACADgAAAAAAAAAaW5wdXQAq6sEAAwAAQABAAEAAAAAAAAAcHNfM18wAE1pY3Jvc29mdCAoUikgSExTTCBTaGFkZXIgQ29tcGlsZXIgMTAuMQCrUQAABQAAD6AAAIA/AAAAAAAAAAAAAAAAHwAAAgUAAIAAAAOQHwAAAgAAAJAACA+gQgAAAwAAD4AAAOSQAAjkoAEAAAIACAuAAADkgAEAAAIACASAAAAAoP//AAA=";
        private readonly PixelShader _shader;

        public CustomShader(string shaderName)
        {
            _shader = new PixelShader();
            using (var stream = new MemoryStream(Convert.FromBase64String(_kshaderAsBase64)))
            {
                _shader.SetStreamSource(stream);
            }
            PixelShader = _shader;
            UpdateShaderValue(InputProperty);
        }

        public Brush Input
        {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        public static readonly DependencyProperty InputProperty =
            ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(CustomShader), 0);

    }
}*/