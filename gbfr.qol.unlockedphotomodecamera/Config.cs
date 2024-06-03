using gbfr.qol.unlockedphotomodecamera.Template.Configuration;

using Reloaded.Mod.Interfaces.Structs;

using System.ComponentModel;

namespace gbfr.qol.unlockedphotomodecamera.Configuration
{
    public class Config : Configurable<Config>
    {
        /*
            User Properties:
                - Please put all of your configurable properties here.
    
            By default, configuration saves as "Config.json" in mod user config folder.    
            Need more config files/classes? See Configuration.cs
    
            Available Attributes:
            - Category
            - DisplayName
            - Description
            - DefaultValue

            // Technically Supported but not Useful
            - Browsable
            - Localizable

            The `DefaultValue` attribute is used as part of the `Reset` button in Reloaded-Launcher.
        */

        [DisplayName("Camera Speed")]
        [Description("Camera Speed. Defaults to 0.5.")]
        [DefaultValue(0.5f)]
        [SliderControlParams(minimum: 0.5f, maximum: 5.0f, tickFrequency: 1,
            isSnapToTickEnabled: true,
            tickPlacement: SliderControlTickPlacement.BottomRight,
            isTextFieldEditable: true)]
        public float CameraSpeed { get; set; } = 1.0f;

        /*
        [DisplayName("Rotation Speed")]
        [Description("Rotation Speed. Defaults to 0.5.")]
        [DefaultValue(0.5f)]
        [SliderControlParams(minimum: 0.0f, maximum: 5.0f, tickFrequency: 1,
            isSnapToTickEnabled: true,
            tickPlacement: SliderControlTickPlacement.BottomRight,
            isTextFieldEditable: true)]
        public float RotationSpeed { get; set; } = 0.5f;
        */


        public enum ELobbyDistanceFilter
        {
            ELobbyDistanceFilterClose,
            ELobbyDistanceFilterDefault,
            ELobbyDistanceFilterFar,
            ELobbyDistanceFilterWorldwide,
        }
    }

    /// <summary>
    /// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
    /// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
    /// </summary>
    public class ConfiguratorMixin : ConfiguratorMixinBase
    {
        // 
    }
}
