using System.Collections.Generic;
using Xunit;

namespace ShaderPlayground.Core.Tests
{
    public class CompilerTests
    {
        private const string HlslCode = @"struct PSInput
{
	float4 color : COLOR;
};

float4 PSMain(PSInput input) : SV_TARGET
{
	return input.color;
}";

        [Fact]
        public void CanCompileHlslToDxbcUsingFxc()
        {
            var result = Compiler.Compile(
                new ShaderCode(LanguageNames.Hlsl, HlslCode),
                new CompilationStep(
                    CompilerNames.Fxc,
                    new Dictionary<string, string>
                    {
                        { "EntryPoint", "PSMain" },
                        { "TargetProfile", "ps_5_0" },
                        { "DisableOptimizations", "false" },
                        { "OptimizationLevel", "2" },
                        { "Version", "10.0.10240.16384" },
                    }))[0];

            Assert.Equal("DXBC", result.Outputs[0].Language);
            Assert.Equal(28, result.Outputs[0].Value.Length);
        }

        [Fact]
        public void CanCompileHlslToDxilUsingDxc()
        {
            var result = Compiler.Compile(
                new ShaderCode(LanguageNames.Hlsl, HlslCode),
                new CompilationStep(
                    CompilerNames.Dxc,
                    new Dictionary<string, string>
                    {
                        { "EntryPoint", "PSMain" },
                        { "TargetProfile", "ps_6_0" },
                        { "DisableOptimizations", "false" },
                        { "ExtraOptions", "" },
                        { "Enable16BitTypes", "false" },
                        { "OptimizationLevel", "2" },
                        { "OutputLanguage", LanguageNames.Dxil },
                        { "Version", "trunk" },
                    }))[0];

            Assert.Equal("DXIL", result.PipeableOutput.Language);
            Assert.Equal(ShaderCodeType.Binary, result.PipeableOutput.CodeType);
            Assert.Equal(2868, result.PipeableOutput.Binary.Length);
            Assert.Equal("Disassembly", result.Outputs[0].DisplayName);
            Assert.Equal("DXIL", result.Outputs[0].Language);
            Assert.Equal(3608, result.Outputs[0].Value.Length);
            Assert.Equal("AST", result.Outputs[1].DisplayName);
            Assert.Equal(1100, result.Outputs[1].Value.Length);
        }

        [Fact]
        public void CanCompileHlslToSpirVUsingDxc()
        {
            var result = Compiler.Compile(
                new ShaderCode(LanguageNames.Hlsl, HlslCode),
                new CompilationStep(
                    CompilerNames.Dxc,
                    new Dictionary<string, string>
                    {
                        { "EntryPoint", "PSMain" },
                        { "TargetProfile", "ps_6_0" },
                        { "DisableOptimizations", "false" },
                        { "ExtraOptions", "" },
                        { "OptimizationLevel", "2" },
                        { "OutputLanguage", LanguageNames.SpirV },
                        { "SpirvTarget", "vulkan1.0" },
                        { "Enable16BitTypes", "false" },
                        { "Version", "trunk" },
                    }))[0];

            Assert.Equal("SPIR-V", result.PipeableOutput.Language);
            Assert.Equal(ShaderCodeType.Binary, result.PipeableOutput.CodeType);
            Assert.Equal(368, result.PipeableOutput.Binary.Length);
            Assert.Equal("SPIR-V", result.Outputs[0].Language);
            Assert.Equal(1145, result.Outputs[0].Value.Length);
        }

        [Fact]
        public void CanCompileHlslToSpirVUsingGlslang()
        {
            var result = Compiler.Compile(
                new ShaderCode(LanguageNames.Hlsl, HlslCode),
                new CompilationStep(
                    CompilerNames.Glslang,
                    new Dictionary<string, string>
                    {
                        { "ShaderStage", "frag" },
                        { "Target", "Vulkan 1.0" },
                        { "EntryPoint", "PSMain" },
                        { "Version", "trunk" },
                    }))[0];

            Assert.Equal("SPIR-V", result.Outputs[0].Language);
            Assert.Equal(1235, result.Outputs[0].Value.Length);
        }

        [Fact]
        public void CanPipeHlslToSpirVToMali()
        {
            var result = Compiler.Compile(
                new ShaderCode(LanguageNames.Hlsl, HlslCode),
                new CompilationStep(
                    CompilerNames.Dxc,
                    new Dictionary<string, string>
                    {
                        { "EntryPoint", "PSMain" },
                        { "TargetProfile", "ps_6_0" },
                        { "OutputLanguage", LanguageNames.SpirV },
                        { "DisableOptimizations", "false" },
                        { "ExtraOptions", "" },
                        { "OptimizationLevel", "2" },
                        { "SpirvTarget", "vulkan1.0" },
                        { "Enable16BitTypes", "false" },
                        { "Version", "trunk" },
                    }),
                new CompilationStep(
                    CompilerNames.Mali,
                    new Dictionary<string, string>
                    {
                        { "ShaderStage", "frag" },
                        { "EntryPoint", "PSMain" },
                        { "Core", "Mali-G72" },
                        { "Version", "6.2.0" },
                    }))[1];

            Assert.Null(result.Outputs[0].Language);
            Assert.Equal(663, result.Outputs[0].Value.Length);
        }
    }
}
