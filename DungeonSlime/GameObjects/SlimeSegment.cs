using Microsoft.Xna.Framework;

namespace DungeonSlime.GameObjects;

public struct SlimeSegment
{
    /// <summary>
    /// The position this slime segment is at fore the movement cycle occurs
    /// </summary>
    public Vector2 At;

    /// <summary>
    /// The position this slime segment should move to during the next movement cycle
    /// </summary>
    public Vector2 To;

    /// <summary>
    /// The direction this slime is moving
    /// </summary>
    public Vector2 Direction;

    /// <summary>
    /// The opposite direction this slime is moving
    /// </summary>
    public Vector2 ReverseDirection => new Vector2(-Direction.X, -Direction.Y);

}
