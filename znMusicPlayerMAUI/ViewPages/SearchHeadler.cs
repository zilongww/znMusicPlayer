﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using znMusicPlayerMAUI.BackgroundCodes.DataEditor;

namespace znMusicPlayerMAUI.ViewPages
{
    public class SongSearchHandler : SearchHandler
    {
        //public IList<Animal> Animals { get; set; }
        public Type SelectedItemNavigationTarget { get; set; }

        protected override void OnQueryChanged(string oldValue, string newValue)
        {
            base.OnQueryChanged(oldValue, newValue);

            if (string.IsNullOrWhiteSpace(newValue))
            {
                ItemsSource = null;
            }
            else
            {
                //ItemsSource = Animals
                //    .Where(animal => animal.Name.ToLower().Contains(newValue.ToLower()))
                //    .ToList<Animal>();
            }
        }

        protected override async void OnItemSelected(object item)
        {
            base.OnItemSelected(item);

            // Let the animation complete
            await Task.Delay(1000);

            ShellNavigationState state = (App.Current.MainPage as Shell).CurrentState;
            // The following route works because route names are unique in this app.
            //await Shell.Current.GoToAsync($"{GetNavigationTarget()}?name={((Animal)item).Name}");
        }

        string GetNavigationTarget()
        {
            return SelectedItemNavigationTarget.ToString();
            //return (Shell.Current as AppShell).Routes.FirstOrDefault(route => route.Value.Equals(SelectedItemNavigationTarget)).Key;
        }
    }
}
