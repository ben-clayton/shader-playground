﻿using System;
using System.IO;

namespace ShaderPlayground.Core.Util
{
    internal sealed class TempFile : IDisposable
    {
        public static TempFile FromShaderCode(ShaderCode shaderCode)
        {
            var result = new TempFile(GetFileExtension(shaderCode.Language));

            switch (shaderCode.CodeType)
            {
                case ShaderCodeType.Text:
                    File.WriteAllText(result.FilePath, shaderCode.Text);
                    break;

                case ShaderCodeType.Binary:
                    File.WriteAllBytes(result.FilePath, shaderCode.Binary);
                    break;

                default:
                    throw new InvalidOperationException();
            }
            
            return result;
        }

        private static string GetFileExtension(string language)
        {
            switch (language)
            {
                case LanguageNames.Slang:
                    return ".slang";

                case LanguageNames.Hlsl:
                    return ".hlsl";

                default:
                    return ".tmp";
            }
        }

        public string FilePath { get; }

        private TempFile(string extension)
        {
            FilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + extension);
        }

        public void Dispose()
        {
            File.Delete(FilePath);
        }

        public static implicit operator string(TempFile tf) => tf.FilePath;
    }
}
