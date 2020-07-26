namespace System
{
    using IL2CPU.API.Attribs;

    [Plug(TargetName = "System.Enum, System.Private.CoreLib")]
    public class EnumImpl
    {
        public static object ToString(Enum aThis, string format) 
            => throw new NotImplementedException($"EnumImpl:ToString {format}");
    }
}