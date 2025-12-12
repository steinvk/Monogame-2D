using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace MonoGameLibrary.Graphics;

/// <summary>
/// Represents a rectangular region within a texture
/// </summary>
public class TextureRegion
{
    /// <summary>
    /// Gets or sets the source texture this texture region is part of
    /// </summary>
    public Texture2D Texture { get; set; }

    /// <summary>
    /// Gets or sets the source rectangle boundary of this texture reagion within the source texture
    /// </summary>
    public Rectangle SourceRectangle { get; set; }

    /// <summary>
    /// Gets the width, in pixels, of this texture region
    /// </summary>
    public int Width => SourceRectangle.Width;

    /// <summary>
    /// Gets the height, in pixels, of this texture region
    /// </summary>
    public int Height => SourceRectangle.Height;

    
    /// <summary>
    /// Gets the top normalized texture coordinate of this region
    /// </summary>
    public float TopTextureCoordinate => SourceRectangle.Top / (float)Texture.Height;

    /// <summary>
    /// Gets the bottom normalized texture coordinate of this region
    /// </summary>
    public float BottomTextureCoordinate => SourceRectangle.Bottom / (float)Texture.Height;

    /// <summary>
    /// Get the left normalized texture coordinate of this region
    /// </summary>
    public float LeftTextureCoordinate => SourceRectangle.Left / (float)Texture.Width;

    /// <summary>
    /// Gets the right normalized texture coordinate of this region
    /// </summary>
    public float RightTextureCoordinate => SourceRectangle.Right / (float)Texture.Width;

    

    /// <summary>
    /// Creates a new region
    /// </summary>
    public TextureRegion() { }

    public TextureRegion(Texture2D texture, int x, int y, int width, int height)
    {
        Texture = texture;
        SourceRectangle = new Rectangle(x, y, width, height);
    }


    public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color)
    {
        Draw(spriteBatch, position, color, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.0f);
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
    {
        Draw(
            spriteBatch,
            position,
            color,
            rotation,
            origin,
            new Vector2(scale, scale),
            effects,
            layerDepth
        );
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
    {
        spriteBatch.Draw(
            Texture,
            position,
            SourceRectangle,
            color,
            rotation,
            origin,
            scale,
            effects,
            layerDepth
        );
    }
}
