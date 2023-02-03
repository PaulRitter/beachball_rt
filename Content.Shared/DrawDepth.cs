using Robust.Shared.Serialization;
using DrawDepthTag = Robust.Shared.GameObjects.DrawDepth;

namespace Content.Shared;

[ConstantsFor(typeof(DrawDepthTag))]
public enum DrawDepth
{
    Background = DrawDepthTag.Default - 1,
    Objects = DrawDepthTag.Default + 1,
    Decals = DrawDepthTag.Default + 2,
}