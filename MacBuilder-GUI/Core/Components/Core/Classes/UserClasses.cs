using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacBuilder_GUI.Core.Components.Core.Classes
{
    public class UserClasses
    {
        public class UserSettings
        {
            public double AnimationSpeed { get; set; }
            public double ColorTransitionDuration { get; set; }
            public double StopMovementDuration { get; set; }
            public string DebugLogPath { get; set; }
            public bool AnimationsEnabled { get; set; }
            public bool DarkModeEnabled { get; set; }
        }
    }
}
