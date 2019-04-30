using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

using static ImGuiNET.ImGuiNative;

namespace ImGuiNET
{
    class Program
    {
        private static Sdl2Window m_window;
        private static GraphicsDevice m_device;
        private static CommandList m_commandList;
        private static ImGuiController m_controller;
        // UI state
        private static float m_floatValue = 0.0f;
        private static Vector3 m_clearColor = new Vector3(0.5f, 0.5f, 0.5f);
        static void SetThing(out float i, float val) { i = val; }

        static void Main(string[] args)
        {
            VeldridStartup.CreateWindowAndGraphicsDevice(
                new WindowCreateInfo(50, 50, 1280, 720, WindowState.Normal, "ImGui.NET"),
                new GraphicsDeviceOptions(true, null, true),
                out m_window,
                out m_device);

            m_window.Resized += () =>
            {
                m_device.MainSwapchain.Resize((uint)m_window.Width, (uint)m_window.Height);
                m_controller.WindowResized(m_window.Width, m_window.Height);
            };

            m_commandList = m_device.ResourceFactory.CreateCommandList();
            m_controller = new ImGuiController(m_device, m_device.MainSwapchain.Framebuffer.OutputDescription, m_window.Width, m_window.Height);

            // Main application loop
            while (m_window.Exists)
            {
                InputSnapshot snapshot = m_window.PumpEvents();
                if (!m_window.Exists) { break; }
                m_controller.Update(1f / 60f, snapshot); // Feed the input events to our ImGui controller, which passes them through to ImGui.

                SubmitUI();

                m_commandList.Begin();
                m_commandList.SetFramebuffer(m_device.MainSwapchain.Framebuffer);
                m_commandList.ClearColorTarget(0, new RgbaFloat(m_clearColor.X, m_clearColor.Y, m_clearColor.Z, 1f));
                m_controller.Render(m_device, m_commandList);
                m_commandList.End();
                m_device.SubmitCommands(m_commandList);
                m_device.SwapBuffers(m_device.MainSwapchain);
            }

            // Clean up Veldrid resources
            m_device.WaitForIdle();
            m_controller.Dispose();
            m_commandList.Dispose();
            m_device.Dispose();
        }

        private static unsafe void SubmitUI()
        {
            // Demo code adapted from the official Dear ImGui demo program:
            // https://github.com/ocornut/imgui/blob/master/examples/example_win32_directx11/main.cpp#L172
            // 1. Show a simple window.
            // Tip: if we don't call ImGui.BeginWindow()/ImGui.EndWindow() the widgets automatically appears in a window called "Debug".
            {
                ImGui.Text("Hello, world!");                                        // Display some text (you can use a format string too)
                ImGui.SliderFloat("float", ref m_floatValue, 0, 1, m_floatValue.ToString("0.000"), 1);  // Edit 1 float using a slider from 0.0f to 1.0f    
                ImGui.ColorEdit3("clear color", ref m_clearColor);                   // Edit 3 floats representing a color
                ImGui.Text($"Mouse position: {ImGui.GetMousePos()}");
                float framerate = ImGui.GetIO().Framerate;
                ImGui.Text($"Application average {1000.0f / framerate:0.##} ms/frame ({framerate:0.#} FPS)");
            }

            ImGuiIOPtr io = ImGui.GetIO();
            SetThing(out io.DeltaTime, 2f);
        }
    }
}
