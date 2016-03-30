﻿using System;
using Android.ViewModels;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.Command;
using Android.Views;
using System.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Android
{
	public class JobsFragmentViewModel : ViewModelBase
	{
		public ObservableCollection<JobItemViewModel> Jobs { get; private set; }

		ObservableCollection<JobItemViewModel> _lastUpdatedJobs;

		public RelayCommand OnJobSelectedCommand;

		public event EventHandler OnJobSelectedEvent;

		public bool IsLoading { get; set; }

		public bool QueryNotFound { get; set; }

		IJobRepository _jobRepository;

		public JobsFragmentViewModel (IJobRepository jobRepository)
		{
			_jobRepository = jobRepository;

			Jobs = new ObservableCollection<JobItemViewModel>();

			OnJobSelectedCommand = new RelayCommand(OnJobSelected);

			IsLoading = false;

			SubscribeToMessages();
		}

		void OnJobSelected ()
		{
			OnJobSelectedEvent(this, null);
		}

		void SubscribeToMessages ()
		{
			MessengerInstance.Register<NotifyJobListUserChangedQuery>(this, OnUserSearch);
			MessengerInstance.Register<NotifyUserClearedText>(this, OnUserClearedText);
		}

		void OnUserClearedText (NotifyUserClearedText pr)
		{
			RemoveAnyState();

			AddToJobs(_lastUpdatedJobs, true);
		}

		async void OnUserSearch(NotifyJobListUserChangedQuery qr)
		{
			var query = qr.Query;

			if(string.IsNullOrEmpty(query))
				return;

			Jobs.Clear();

			ShowLoadingState();

			await Task.Delay(2000);

			ShowEmptyState();
		}

		void ShowEmptyState()
		{
			IsLoading = false;

			QueryNotFound = true;
		}

		void ShowLoadingState()
		{
			IsLoading = true;

			QueryNotFound = false;
		}

		void RemoveAnyState ()
		{
			IsLoading = false;

			QueryNotFound = false;
		}

		public override async void OnCreate()
		{
			if(!Jobs.Any())
			{
				await GetMostRecentJobs();
			}
		}

		async Task GetMostRecentJobs ()
		{
			IsLoading = true;

			_lastUpdatedJobs = await _jobRepository.GetMostRecentJobs ();

			AddToJobs(_lastUpdatedJobs);
		}

		void AddToJobs(IEnumerable<JobItemViewModel> recentJobs, bool clear = false)
		{
			if(clear)
				Jobs.Clear();
			
			foreach(var item in recentJobs)
			{
				Jobs.Add(item);
			}

			IsLoading = false;
		}

		public override void OnStop ()
		{
		}

		public override void OnResume ()
		{
			
		}

		protected virtual void OnOnJobSelectedEvent (EventArgs e)
		{
			var handler = OnJobSelectedEvent;
			if (handler != null)
				handler (this, e);
		}
	}
}

