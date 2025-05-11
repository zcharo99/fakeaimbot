using ImGuiNET;
using ClickableTransparentOverlay;
using System.Numerics;
using System;
using System.Runtime.InteropServices;

namespace fakeaimbot
{
    public class Program : Overlay
    {
        static bool showWindow = true;
        static bool keyHeld = false;
        static bool overlay = false;
        static int radius = 300;
        static float chromaSpeed = 0.25f;
        static Vector2 screenSize;
        static Vector2 drawPosition;

        protected override void Render()
        {
            DrawMenu();
            DrawOverlay();
        }

        Vector4 HsvToRgb(float h, float s, float v)
        {
            float r = 0, g = 0, b = 0;

            h *= 6;
            int i = (int)MathF.Floor(h);
            float f = h - i;
            float p = v * (1 - s);
            float q = v * (1 - f * s);
            float t = v * (1 - (1 - f) * s);

            switch (i % 6)
            {
                case 0: r = v; g = t; b = p; break;
                case 1: r = q; g = v; b = p; break;
                case 2: r = p; g = v; b = t; break;
                case 3: r = p; g = q; b = v; break;
                case 4: r = t; g = p; b = v; break;
                case 5: r = v; g = p; b = q; break;
            }

            return new Vector4(r, g, b, 1.0f); // alpha = 1 nya~
        }

        void DrawMenu()
        {
            if ((GetAsyncKeyState(0x2D) & 0x8000) != 0 && !keyHeld)
            {
                showWindow = !showWindow;
                keyHeld = true;
            }

            if (!((GetAsyncKeyState(0x2D) & 0x8000) != 0))
            {
                keyHeld = false;
            }

            if (showWindow) {
                ImGui.Begin("fakeaimbot");
                if (ImGui.Button("Exit"))
                {
                    Environment.Exit(0);
                }
                ImGui.Separator();
                if (ImGui.Button("Reset Circle Radius"))
                {
                    radius = 300;
                }
                if (ImGui.Button("Reset Chroma Speed"))
                {
                    chromaSpeed = 0.25f;
                }
                ImGui.Separator();
                ImGui.Text("Press INSERT to toggle this menu");
                ImGui.Checkbox("Overlay", ref overlay);
                ImGui.SliderInt("Circle Radius", ref radius, 100, 540, "%d");
                ImGui.SliderFloat("Chroma Speed", ref chromaSpeed, 0.01f, 1.0f, "%.2f");
                ImGui.End();
            }
        }

        void DrawOverlay()
        {
            if (overlay)
            {
                ImGui.SetNextWindowSize(screenSize);
                ImGui.SetNextWindowPos(new Vector2(0, 0));
                ImGui.Begin("Overlay", ImGuiWindowFlags.NoDecoration
                | ImGuiWindowFlags.NoBackground
                | ImGuiWindowFlags.NoBringToFrontOnFocus
                | ImGuiWindowFlags.NoMove
                | ImGuiWindowFlags.NoInputs
                | ImGuiWindowFlags.NoCollapse
                | ImGuiWindowFlags.NoScrollbar
                | ImGuiWindowFlags.NoScrollWithMouse
                );

                ImDrawListPtr drawList = ImGui.GetWindowDrawList();

                float time = (float)ImGui.GetTime();
                float hue = (time * chromaSpeed) % 1.0f;

                Vector4 rainbowColor = HsvToRgb(hue, 1f, 1f);
                uint color = ImGui.ColorConvertFloat4ToU32(rainbowColor);

                drawList.AddCircle(drawPosition, radius, color);
            }
        }

        public static void Main(string[] args)
        {
            int width = GetSystemMetrics(0);
            int height = GetSystemMetrics(1);

            screenSize = new Vector2(width, height);
            drawPosition = screenSize / 2;

            Program program = new Program();
            program.Start().Wait();
        }

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(int nIndex);
    }
}