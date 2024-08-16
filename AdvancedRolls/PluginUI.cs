using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AdvancedRolls
{
    public class PluginUI
    {
        public bool IsVisible;
        public bool ShowSupport;
        public void Draw()
        {
            if (!IsVisible || !ImGui.Begin("Advanced Rolls Config", ref IsVisible, ImGuiWindowFlags.AlwaysAutoResize))
                return;
            ImGui.Text("Text Example");
            ImGui.SetNextItemWidth(310);


            if (ImGui.Button("Save"))
            {
                Plugin.PluginConfig.Save();
                this.IsVisible = false;
            }
            ImGui.End();
        }
    }
}
