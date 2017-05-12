using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace CarvHazard
{

    public class SoundManager
    {
        public SoundEffect explodeSound;
        public Song bgMusic;

        public SoundManager()
        {
            explodeSound = null;
            bgMusic = null;
        }

        public void LoadContent(ContentManager Content)
        {
            explodeSound = Content.Load<SoundEffect>("ImgAud/metal");
            bgMusic = Content.Load<Song>("ImgAud/Roadbg");
        }

    }

}