﻿// License :
//
// SoundTouch audio processing library
// Copyright (c) Olli Parviainen
// C# port Copyright (c) Olaf Woudenberg
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

namespace znMusicPlayerWUI.Media.AudioEffects
{
    using System;

    /// <summary>
    /// Provides data for the event that is raised when a fault happened during
    /// the Read operation of the <see cref="SoundTouchWaveProvider"/>.
    /// </summary>
    public class UnobservedExceptionEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnobservedExceptionEventArgs"/> class.
        /// </summary>
        /// <param name="exception">The Exception that was caught.</param>
        public UnobservedExceptionEventArgs(Exception exception)
        {
            Exception = exception;
        }

        /// <summary>
        /// Gets the Exception that was caught.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Gets a value indicating whether this exception has been marked as "observed.".
        /// </summary>
        public bool IsObserved { get; private set; }

        /// <summary>
        /// Marks the <see cref="Exception"/> as "observed", thus preventing it from triggering
        /// an exception in the <see cref="NAudio.Wave.IWaveProvider.Read(byte[], int, int)"/> method.
        /// </summary>
        public void SetObserved() => IsObserved = true;
    }
}