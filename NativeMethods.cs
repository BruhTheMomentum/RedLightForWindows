using System.Runtime.InteropServices;

namespace RedLight;

internal static class NativeMethods
{
    private const string MagnificationDll = "Magnification.dll";

    public static readonly float[] IdentityMatrix =
    [
        1, 0, 0, 0, 0,
        0, 1, 0, 0, 0,
        0, 0, 1, 0, 0,
        0, 0, 0, 1, 0,
        0, 0, 0, 0, 1
    ];

    [DllImport(MagnificationDll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool MagInitialize();

    [DllImport(MagnificationDll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool MagUninitialize();

    [DllImport(MagnificationDll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool MagSetFullscreenColorEffect(ref MagColorEffect pEffect);

    public static void ResetColorEffect()
    {
        var effect = new MagColorEffect(IdentityMatrix);
        MagSetFullscreenColorEffect(ref effect);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MagColorEffect
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 25)]
        public float[] transform;

        public MagColorEffect(float[] matrix)
        {
            transform = new float[25];
            matrix.CopyTo(transform, 0);
        }
    }
}
