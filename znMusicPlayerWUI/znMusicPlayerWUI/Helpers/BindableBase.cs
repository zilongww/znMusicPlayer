﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TewIMP.DataEditor;

namespace TewIMP.Helpers
{
    public class SongItemBindBase : BindableBase
    {
        MusicData _musicData;
        public MusicData MusicData
        {
            get => _musicData;
            set
            {
                if (_musicData != value)
                {
                    _musicData = value;
                    OnPropertyChanged(nameof(MusicData));
                }
            }
        }

        MusicListData _musicListData;
        public MusicListData MusicListData
        {
            get => _musicListData;
            set
            {
                if (_musicListData != value)
                {
                    _musicListData = value;
                    OnPropertyChanged(nameof(MusicListData));
                }
            }
        }

        double _imageScaleDPI = 1.0;
        public double ImageScaleDPI
        {
            get => _imageScaleDPI;
            set
            {
                if (_imageScaleDPI != value)
                {
                    _imageScaleDPI = value;
                    OnPropertyChanged(nameof(ImageScaleDPI));
                }
            }
        }

        bool _showAlbumName = true;
        public bool ShowAlbumName
        {
            get => _showAlbumName;
            set
            {
                _showAlbumName = value;
                OnPropertyChanged(nameof(ShowAlbumName));
            }
        }

        string _searchText = null;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
            }
        }
    }

    /// <summary>
    /// Implementation of <see cref="INotifyPropertyChanged"/> to simplify models.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class BindableBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Multicast event for property change notifications.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Checks if a property already matches a desired value.  Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (object.Equals(storage, value)) return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="CallerMemberNameAttribute"/>.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}