﻿/*The contents of this file are subject to the Mozilla Public License Version 1.1
(the "License"); you may not use this file except in compliance with the
License. You may obtain a copy of the License at http://www.mozilla.org/MPL/

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

The Original Code is the TSOClient.

The Initial Developer of the Original Code is
ddfczm. All Rights Reserved.

Contributor(s): ______________________________________.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using TSO.Common.rendering.framework;

namespace TSO.Vitaboy
{
    /// <summary>
    /// An animator is used to animate an avatar.
    /// </summary>
    public class Animator : _3DComponent
    {
        protected List<AnimationHandle> Animations = new List<AnimationHandle>();

        /// <summary>
        /// Runs an animation.
        /// </summary>
        /// <param name="avatar">The avatar to run animation for.</param>
        /// <param name="animation">The animation to run.</param>
        /// <returns>Handle to the animation run.</returns>
        public AnimationHandle RunAnimation(Avatar avatar, Animation animation)
        {
            var instance = new AnimationHandle(this);
            instance.Animation = animation;
            instance.Avatar = avatar;
            
            Animations.Add(instance);
            return instance;
        }

        /// <summary>
        /// Disposes an animation.
        /// </summary>
        /// <param name="animation">The animation to dispose.</param>
        public void DisposeAnimation(AnimationHandle animation)
        {
            this.Animations.Remove(animation);
        }

        public void Update(GameTime time)
        {
            lock (Animations)
            {
                for (var i = 0; i < Animations.Count; i++)
                {
                    var item = Animations[i];
                    item.Update(time);
                    if (item.Status == AnimationStatus.COMPLETED)
                    {
                        Animations.RemoveAt(i);
                        i--;
                    }
                }

                //AnimationStatus
            }
        }

        public override void Update(TSO.Common.rendering.framework.model.UpdateState state)
        {
            this.Update(state.Time);
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.GraphicsDevice device)
        {
        }

        /// <summary>
        /// Renders an animation's frame.
        /// </summary>
        /// <param name="avatar">The avatar which the animation is run for.</param>
        /// <param name="animation">The animation.</param>
        /// <param name="frame">Frame number in animation.</param>
        /// <param name="fraction"></param>
        /// <returns>Status of animation.</returns>
        public static AnimationStatus RenderFrame(Avatar avatar, Animation animation, int frame, float fraction)
        {
            if (frame < 0 || frame > animation.NumFrames) return AnimationStatus.COMPLETED;

            var numDone = 0;

            foreach (var motion in animation.Motions)
            {
                var bone = avatar.Skeleton.GetBone(motion.BoneName);
                if (bone == null) continue; //fixes bugs with missing bones.. need to find out what R_FINGERPOLY0 is though.

                var motionFrame = frame;
                if (frame >= motion.FrameCount)
                {
                    numDone++;
                    motionFrame = (int)motion.FrameCount - 1;
                }

                if (motion.HasTranslation)
                {
                    if (fraction >= 0)
                    {
                        var trans1 = animation.Translations[motion.FirstTranslationIndex + motionFrame];
                        var trans2 = (frame + 1 >= motion.FrameCount) ? trans1 : animation.Translations[motion.FirstTranslationIndex + motionFrame+1];
                        bone.Translation = Vector3.Lerp(trans1, trans2, fraction);
                    }
                    else
                    {
                        bone.Translation = animation.Translations[motion.FirstTranslationIndex + motionFrame];
                    }
                }
                if (motion.HasRotation)
                {
                    if (fraction >= 0)
                    {
                        var quat1 = animation.Rotations[motion.FirstRotationIndex + motionFrame];
                        var quat2 = (frame + 1 >= motion.FrameCount) ? quat1 : animation.Rotations[motion.FirstRotationIndex + motionFrame + 1];
                        bone.Rotation = Quaternion.Slerp(quat1, quat2, fraction);
                    }
                    else
                    {
                        bone.Rotation = animation.Rotations[motion.FirstRotationIndex + motionFrame];
                    }
                }
            }

            avatar.ReloadSkeleton();
            return AnimationStatus.IN_PROGRESS;
        }

        public override void DeviceReset(Microsoft.Xna.Framework.Graphics.GraphicsDevice Device)
        {
        }
    }

    /// <summary>
    /// Handle to an animation.
    /// </summary>
    public class AnimationHandle
    {
        public Animation Animation;
        public double Speed = 1.0f;
        public Avatar Avatar;
        public long StartTime;
        private Animator Owner;
        public AnimationStatus Status;
        
        /// <summary>
        /// Constructs a new AnimationHandle instance.
        /// </summary>
        /// <param name="animator">The Animator instance to use.</param>
        public AnimationHandle(Animator animator)
        {
            this.Owner = animator;
        }

        /// <summary>
        /// Disposes this animation handle.
        /// </summary>
        public void Dispose()
        {
            this.Owner.DisposeAnimation(this);
        }

        public void Update(GameTime time)
        {
            var now = time.ElapsedGameTime.Milliseconds;
            if (this.Status == AnimationStatus.WAITING_TO_START){
                StartTime = now;
                this.Status = AnimationStatus.IN_PROGRESS;
            }

            var fps = 30.0f * Speed;
            var fpms = fps / 1000;

            var runTime = now - StartTime;
            uint frame = (uint)(runTime * fpms);
            var fraction = (runTime * fpms) - frame;

            int numDone = 0;

            /** Speed is 30fps by default **/
            foreach (var motion in Animation.Motions)
            {
                var bone = Avatar.Skeleton.GetBone(motion.BoneName);
                var motionFrame = frame;
                if (frame >= motion.FrameCount)
                {
                    numDone++;
                    motionFrame = motion.FrameCount - 1;
                }

                if (motion.HasTranslation)
                {
                    bone.Translation = Animation.Translations[motion.FirstTranslationIndex + motionFrame];
                }
                if (motion.HasRotation)
                {
                    bone.Rotation = Animation.Rotations[motion.FirstRotationIndex + motionFrame];
                }
            }

            if (numDone == Animation.Motions.Length)
            {
                /** Completed! **/
                this.Status = AnimationStatus.COMPLETED;
            }
            else
            {
                Avatar.ReloadSkeleton();
            }
        }
    }

    public enum AnimationStatus
    {
        WAITING_TO_START,
        IN_PROGRESS,
        COMPLETED,
        STOPPED
    }
}
