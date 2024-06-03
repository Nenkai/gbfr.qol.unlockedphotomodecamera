using System.Diagnostics;

using Reloaded.Mod.Interfaces;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X64;
using Reloaded.Memory.Sigscan;
using Reloaded.Memory.Sigscan.Definitions.Structs;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;

using gbfr.qol.unlockedphotomodecamera.Configuration;
using gbfr.qol.unlockedphotomodecamera.Template;
using Reloaded.Memory;
using Reloaded.Memory.Interfaces;
using Reloaded.Hooks.ReloadedII.Interfaces;
using System.Runtime.InteropServices;
using System.Text;
using System.Buffers.Binary;


namespace gbfr.qol.unlockedphotomodecamera;

/// <summary>
/// Your mod logic goes here.
/// </summary>
public unsafe class Mod : ModBase // <= Do not Remove.
{
    /// <summary>
    /// Provides access to the mod loader API.
    /// </summary>
    private readonly IModLoader _modLoader;

    /// <summary>
    /// Provides access to the Reloaded.Hooks API.
    /// </summary>
    /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
    private readonly IReloadedHooks? _hooks;

    /// <summary>
    /// Provides access to the Reloaded logger.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Entry point into the mod, instance that created this class.
    /// </summary>
    private readonly IMod _owner;

    /// <summary>
    /// Provides access to this mod's configuration.
    /// </summary>
    private Config _configuration;

    /// <summary>
    /// The configuration of the currently executing mod.
    /// </summary>
    private readonly IModConfig _modConfig;

    private static IStartupScanner? _startupScanner = null!;

    private IHook<PhotoParamBaseCtorDelegate> _photoParamCtorHook;
    public delegate void PhotoParamBaseCtorDelegate(PhotoParamBase* @this);

    public Mod(ModContext context)
    {
        _modLoader = context.ModLoader;
        _hooks = context.Hooks;
        _logger = context.Logger;
        _owner = context.Owner;
        _configuration = context.Configuration;
        _modConfig = context.ModConfig;

        var startupScannerController = _modLoader.GetController<IStartupScanner>();
        if (startupScannerController == null || !startupScannerController.TryGetTarget(out _startupScanner))
        {
            return;
        }

#if DEBUG
        // NOTE (Nenkai): Requires steamless/unpacked exe
        // Do not have other mods debugged at the same time
        Debugger.Launch();
#endif

        var memory = Reloaded.Memory.Memory.Instance;

        // Overrides photo parameter constructor with our own through hook
        SigScan("55 41 57 41 56 41 55 41 54 56 57 53 48 81 EC ?? ?? ?? ?? 48 8D AC 24 ?? ?? ?? ?? 48 C7 45 ?? ?? ?? ?? ?? 49 89 CE C7 01", "", address =>
        {
            _photoParamCtorHook = _hooks!.CreateHook<PhotoParamBaseCtorDelegate>(PhotoParamBase_Ctor_Hook, address).Activate();
            _logger.WriteLine($"[{_modConfig.ModId}] Successfully hooked PhotoParamBase Constructor (0x{address:X8})", _logger.ColorGreen);
        });

        // Whatever this does, it's overwriting the bounding cylinder height. Character related value maybe??
        SigScan("C5 F8 11 48 ?? C5 F8 11 00 C5 F8 11 58 ?? C5 F8 11 50 ?? 8B 4D ?? 89 48 ?? 48 8D 75 ?? 48 89 F9 E8 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? 48 8D 55 ?? E8 ?? ?? ??" +
            " ?? 48 8B 0D ?? ?? ?? ?? 48 8D 55 ?? E8 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? 48 8D 55 ?? E8 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? 48 89 F2 E8 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? 48 8D 55 ?? E8 ?? ?? ?? ?? 48 8B 35", "", address =>
        {
            memory.SafeWrite((nuint)address,      new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90 }); // vmovups xmmword ptr [rax+10h], xmm3 -> nop's
            memory.SafeWrite((nuint)address + 5,  new byte[] { 0x90, 0x90, 0x90, 0x90       }); // vmovups xmmword ptr [rax], xmm3 -> nop's
            memory.SafeWrite((nuint)address + 9,  new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90 }); // vmovups xmmword ptr [rax+30h], xmm3 -> nop's
            memory.SafeWrite((nuint)address + 14, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90 }); // vmovups xmmword ptr [rax+20h], xmm3 -> nop's

            _logger.WriteLine($"[{_modConfig.ModId}] Applied height override (0x{address:X8})", _logger.ColorGreen);
        });

        // This overrides camera/rot speed back with 0.5..
        SigScan("C5 FA 10 45 ?? C5 FA 11 47 ?? C5 FA 10 45 ?? C5 FA 11 47 ?? C5 FA 10 45 ?? C5 FA 11 47 ?? C5 FA 10 45 ?? C5 FA 11 47 ?? C5 FA 10 45 ?? C5 FA 11 47 ?? 80 7D", "", address =>
        {
            memory.SafeWrite((nuint)address,      new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90 }); // vmovss  xmm0, dword ptr [rbp+110h+var_12C+4] -> nop
            memory.SafeWrite((nuint)address + 5,  new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90 }); // vmovss  dword ptr [rdi+20h], xmm0 -> nop
            memory.SafeWrite((nuint)address + 10, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90 }); // vmovss  xmm0, dword ptr [rbp+110h+var_12C+8] -> nop
            memory.SafeWrite((nuint)address + 15, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90 }); // vmovss  dword ptr [rdi+24h], xmm0 -> nop

            _logger.WriteLine($"[{_modConfig.ModId}] Applied camera speed override (0x{address:X8})", _logger.ColorGreen);
        });

        // 55 41 57 41 56 41 55 41 54 56 57 53 48 81 EC ?? ?? ?? ?? 48 8D AC 24 ?? ?? ?? ?? C5 78 29 95 ?? ?? ?? ?? C5 78 29 8D ?? ?? ?? ?? C5 79 7F 85
        // ^ this function gets hardcoded height or something maybe? sometimes even depends on char

        // Address of the HitTest function call, ignore it so we can no-clip.
        // Also geez this is large.
        SigScan("E8 ?? ?? ?? ?? 85 C0 0F 84 ?? ?? ?? ?? C5 C8 5C 44 24 ?? C5 F8 28 4C 24 ?? C4 E3 79 40 C1 ?? C5 FA 10 15 ?? ?? ?? ?? C5 EA 5C C0 C4 E3 79 04 C0 ?? C5 F0 " +
            "59 C0 C5 C8 58 C0 C5 F8 29 44 24 ?? C7 44 24 ?? ?? ?? ?? ?? C5 F8 57 C0 C5 F8 29 44 24 ?? C7 44 24 ?? ?? ?? ?? ?? C5 F8 29 44 24 ?? C7 44 24 ?? ?? ?? ?? " +
            "?? C7 44 24 ?? ?? ?? ?? ?? C5 F8 28 0D ?? ?? ?? ?? C5 F8 11 4C 24 ?? C7 84 24 ?? ?? ?? ?? ?? ?? ?? ?? C5 F8 29 84 24 ?? ?? ?? ?? 48 8D 4C 24 ?? 48 8D 54 " +
            "24 ?? C5 FA 10 15 ?? ?? ?? ?? 49 89 F9 E8 ?? ?? ?? ?? 85 C0 74 ?? C5 F8 28 03 C5 F8 29 44 24", "", address =>
        {
            _logger.WriteLine($"[gbfr.qol.unlockedphotomodecamera] Found HitTest call (0x{address:X8})", _logger.ColorGreen);
            memory.SafeWrite((nuint)address, new byte[] { 0xB8, 0x00, 0x00, 0x00, 0x00 }); // call HitTest -> mov eax, 0
        });


        // seems to be default values part of globals that get overwritten by the current photo param, therefore don't need to be edited.
        /*
        SigScan("00 00 20 41 00 00 A0 40 00 00 A0 C0 00 00 48 42", "", address =>
        {
            _logger.WriteLine($"[gbfr.qol.unlockedphotomodecamera] Registered GiveStoneItem (0x{address:X8})", _logger.ColorGreen);

            PhotoParamBase* paramBase = (PhotoParamBase*)(address - 0x30);
            // ...
        });
        */
        
    }

    public void PhotoParamBase_Ctor_Hook(PhotoParamBase* @this)
    {
        _photoParamCtorHook.OriginalFunction(@this);

        @this->CameraSpeed = _configuration.CameraSpeed;
        @this->BoundaryCylinderWidth = 100000.0f;
        @this->BoundaryCylinderHeight = 100000.0f;
        @this->BoundaryCylinderDepth = -100000.0f;
    }

    private void SigScan(string pattern, string name, Action<nint> action)
    {
        nint baseAddress = Process.GetCurrentProcess().MainModule.BaseAddress;
        _startupScanner?.AddMainModuleScan(pattern, result =>
        {
            if (!result.Found)
            {
                return;
            }
            action(baseAddress + result.Offset);
        });
    }

    #region Standard Overrides
    public override void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
        _configuration = configuration;
        _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
    }
    #endregion

    #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod() { }
#pragma warning restore CS8618
    #endregion
}