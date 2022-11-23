// Spritedow Animation Plugin by Elendow
// https://elendow.com

using System;
using UnityEngine;

namespace Elendow.SpritedowAnimator
{
    [Serializable]
    public class SpriteAnimationAction
    {
        #region Atributes
        [SerializeField]
        private int frame;
        [SerializeField]
        private string data;
        #endregion

        /// <summary>A class to serialize actions on a animation frame.</summary>
        /// <param name="frame">The frame to call the action.</param>
        /// <param name="data">A string defining the data of the action.</param>
        public SpriteAnimationAction(int frame, string data)
        {
            this.frame = frame;
            this.data = data;
        }

        #region Properties
        /// <summary>The frame to call the action.</summary>
        public int Frame
        {
            get => frame;
        }

        /// <summary>The data that carries this action.</summary>
        public string Data
        {
            get => data;
            set => data = value;
        }
        #endregion
    }
}