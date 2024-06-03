using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gbfr.qol.unlockedphotomodecamera;

public struct PhotoParamBase
{
    public int field_0x00;
    public int field_0x04;
    public int field_0x08;
    public float FieldOfView;
    public int field_0x10;
    public int field_0x14;
    public int field_0x18;
    public int field_0x1C;
    public float CameraSpeed;
    public float RotationSpeed;
    public int field_0x28;
    public float field_0x2C;
    public float BoundaryCylinderWidth;
    public float BoundaryCylinderHeight;
    public float BoundaryCylinderDepth;
    public float field_0x3C;

    public PhotoParamBase()
    {
        // These are defaults from the game code
        field_0x00 = 0;
        field_0x04 = 0;
        field_0x08 = 0;
        FieldOfView = 0.872f;
        field_0x10 = 0x1000000;
        field_0x14 = 0;
        field_0x18 = 0;
        field_0x1C = 0x101;
        CameraSpeed = 0.5f;
        RotationSpeed = 0.5f;
        field_0x28 = 1;
        field_0x2C = 0.3f;
        BoundaryCylinderWidth = 10.0f;
        BoundaryCylinderHeight = 5.0f;
        BoundaryCylinderDepth = -5.0f;
        field_0x3C = 50.0f;
    }
}
