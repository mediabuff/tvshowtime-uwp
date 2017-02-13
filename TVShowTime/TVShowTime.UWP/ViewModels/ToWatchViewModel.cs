﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Toolkit.Uwp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TVShowTime.UWP.Constants;
using TVShowTime.UWP.Services;
using TVShowTimeApi.Model;
using TVShowTimeApi.Services;

namespace TVShowTime.UWP.ViewModels
{
    public class ToWatchViewModel : ViewModelBase
    {
        #region Fields

        private IReactiveTVShowTimeApiService _tvshowtimeApiService;
        private IHamburgerMenuService _hamburgerMenuService;
        private IEventService _eventService;

        #endregion

        #region Properties

        public ObservableCollection<Episode> Episodes { get; } = new ObservableCollection<Episode>();

        #endregion

        #region Commands

        public ICommand SelectEpisodeCommand { get; private set; }

        #endregion

        #region Constructor

        public ToWatchViewModel(
            IReactiveTVShowTimeApiService tvshowtimeApiService,
            IHamburgerMenuService hamburgerMenuService,
            IEventService eventService)
        {
            _tvshowtimeApiService = tvshowtimeApiService;
            _hamburgerMenuService = hamburgerMenuService;
            _eventService = eventService;

            SelectEpisodeCommand = new RelayCommand<Episode>(SelectEpisode);

            Refresh();

            _eventService.WatchEpisodeEvent
                .Subscribe((episode) =>
                {
                    Refresh();
                });

            _eventService.UnwatchEpisodeEvent
               .Subscribe((episode) =>
               {
                   Refresh();
               });
        }

        #endregion

        #region Command Methods

        public void SelectEpisode(Episode episode)
        {
            ServiceLocator.Current.GetInstance<EpisodeViewModel>().SelectEpisode(episode);
            _hamburgerMenuService.NavigateTo(ViewConstants.Episode);
        }

        #endregion

        #region Private Methods

        private void Refresh()
        {
            _tvshowtimeApiService.GetWatchlist(0, 50)
                .Subscribe(async (watchlistResponse) =>
                {
                    await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                    {
                        Episodes.Clear();

                        foreach (var episode in watchlistResponse.Episodes)
                        {
                            Episodes.Add(episode);
                        }
                    });
                },
                (error) =>
                {
                    throw new Exception();
                });
        }

        #endregion
    }
}