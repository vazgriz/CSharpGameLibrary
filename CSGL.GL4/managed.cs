using System;
using System.Text;
using System.Runtime.InteropServices;

using CSGL.GL4;
using CSGL.Graphics;
using static CSGL.GL4.Unmanaged.GL;

namespace CSGL.GL4 {
    public static partial class GL {
        public static void BufferData<T>(BufferTarget target, T[] data, BufferUsage usage)  where T : struct {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);    //can't get * pointer of generic type
            int size = Marshal.SizeOf<T>();
            glBufferData(target, size * data.Length, handle.AddrOfPinnedObject(), usage);
            handle.Free();
        }

        public static void BufferSubData<T>(BufferTarget target, int offset, T[] data) where T : struct {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            int size = Marshal.SizeOf<T>();
            glBufferSubData(target, offset, size * data.Length, handle.AddrOfPinnedObject());
            handle.Free();
        }

        public static uint CreateBuffer() {
            uint[] buffers = new uint[1];
            glCreateBuffers(1, buffers);
            return buffers[0];
        }

        public static uint CreateFramebuffer() {
            uint[] framebuffers = new uint[1];
            glCreateFramebuffers(1, framebuffers);
            return framebuffers[0];
        }

        public static uint CreateProgramPipeline() {
            uint[] pipelines = new uint[1];
            glCreateProgramPipelines(1, pipelines);
            return pipelines[0];
        }

        public static uint CreateQuery(QueryMode target) {
            uint[] ids = new uint[1];
            glCreateQueries(target, 1, ids);
            return ids[0];
        }

        public static uint CreateRenderbuffer() {
            uint[] renderbuffers = new uint[1];
            glCreateRenderbuffers(1, renderbuffers);
            return renderbuffers[0];
        }

        public static uint CreateSampler() {
            uint[] samplers = new uint[1];
            glCreateSamplers(1, samplers);
            return samplers[0];
        }

        public static uint CreateShaderProgram(ShaderType type, string source) {
            string[] strings = new string[] { source };
            return glCreateShaderProgramv(type, 1, strings);
        }

        public static uint CreateTexture(TextureTarget target) {
            uint[] textures = new uint[1];
            glCreateTextures(target, 1, textures);
            return textures[0];
        }

        public static uint CreateTransformFeedback() {
            uint[] ids = new uint[1];
            glCreateTransformFeedbacks(1, ids);
            return ids[0];
        }

        public static uint CreateVertexArray() {
            uint[] buffers = new uint[1];
            glCreateVertexArrays(1, buffers);
            return buffers[0];
        }

        public static uint GenBuffer() {
            uint[] buffers = new uint[1];
            glGenBuffers(1, buffers);
            return buffers[0];
        }

        public static uint GenVertexArray() {
            uint[] vao = new uint[1];
            glGenVertexArrays(1, vao);
            return vao[0];
        }

        public static void DrawElements(PrimitiveType mode, int count, byte[] indices) {
            GCHandle handle = GCHandle.Alloc(indices);
            glDrawElements(mode, count, DrawElementsType.UnsignedByte, handle.AddrOfPinnedObject());
            handle.Free();
        }

        public static void DrawElements(PrimitiveType mode, int count, short[] indices) {
            GCHandle handle = GCHandle.Alloc(indices);
            glDrawElements(mode, count, DrawElementsType.UnsignedShort, handle.AddrOfPinnedObject());
            handle.Free();
        }

        public static void DrawElements(PrimitiveType mode, int count, uint[] indices) {
            GCHandle handle = GCHandle.Alloc(indices);
            glDrawElements(mode, count, DrawElementsType.UnsignedInt, handle.AddrOfPinnedObject());
            handle.Free();
        }

        public static int GetProgram(uint id, ProgramParameter pname) {
            int[] value = new int[1];
            glGetProgramiv(id, pname, value);
            return value[0];
        }

        public static string GetProgramInfoLog(uint id) {
            int length = 0;
            StringBuilder builder = new StringBuilder(512);
            glGetProgramInfoLog(id, 512, ref length, builder);
            return builder.ToString();
        }

        public static int GetShader(uint id, ShaderParameter pname) {
            int[] value = new int[1];
            glGetShaderiv(id, pname, value);
            return value[0];
        }

        public static string GetShaderInfoLog(uint id) {
            int length = 0;
            StringBuilder builder = new StringBuilder(512);
            glGetShaderInfoLog(id, 512, ref length, builder);
            return builder.ToString();
        }

        public static void NamedBufferData<T>(uint buffer, T[] data, BufferUsage usage) where T : struct {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            int size = Marshal.SizeOf<T>();
            glNamedBufferData(buffer, size * data.Length, handle.AddrOfPinnedObject(), usage);
            handle.Free();
        }

        public static void NamedBufferSubData<T>(uint buffer, int offset, T[] data) where T : struct {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            int size = Marshal.SizeOf<T>();
            glNamedBufferSubData(buffer, offset, data.Length * size, handle.AddrOfPinnedObject());
            handle.Free();
        }

        public static void ShaderSource(uint id, string source) {
            glShaderSource(id, 1, new string[] { source }, new int[] { source.Length });
        }

        public static void ShaderSource(uint id, string[] sources) {
            int[] lengths = new int[sources.Length];
            for (int i = 0; i < sources.Length; i++) {
                lengths[i] = sources[i].Length;
            }
            glShaderSource(id, sources.Length, sources, lengths);
        }

        public static void VertexAttribPointer(uint id, int size, VertexAttribPointerType type, bool normalized, int stride, int offset) {
            glVertexAttribPointer(id, size, type, normalized, stride, (IntPtr)offset);
        }
    }
}
