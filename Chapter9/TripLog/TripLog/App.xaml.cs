﻿using System;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Xamarin.Forms;
using Xamarin.Essentials;
using Ninject;
using Ninject.Modules;
using TripLog.Services;
using TripLog.Views;
using TripLog.ViewModels;
using TripLog.Modules;

namespace TripLog
{
    public partial class App : Application
    {
        bool IsSignedIn => !string.IsNullOrWhiteSpace(Preferences.Get("apitoken", ""));

        public IKernel Kernel { get; set; }

        public App(params INinjectModule[] platformModules)
        {
            InitializeComponent();

            // Register core services
            Kernel = new StandardKernel(
                new TripLogCoreModule(),
                new TripLogNavModule());

            // Register platform specific services
            Kernel.Load(platformModules);

            // Setup data service authentication delegates
            var dataService = Kernel.Get<ITripLogDataService>();
            dataService.AuthorizedDelegate = OnSignIn;
            dataService.UnauthorizedDelegate = SignOut;

            SetMainPage();
        }

        void SetMainPage()
        {
            var mainPage = IsSignedIn
                ? new NavigationPage(new MainPage())
                    {
                        BindingContext = Kernel.Get<MainViewModel>()
                    }
                : new NavigationPage(new SignInPage())
                    {
                        BindingContext = Kernel.Get<SignInViewModel>()
                    };

            var navService = Kernel.Get<INavService>() as XamarinFormsNavService;

            navService.XamarinFormsNav = mainPage.Navigation;

            MainPage = mainPage;
        }

        void OnSignIn(string accessToken)
        {
            Preferences.Set("apitoken", accessToken);

            SetMainPage();
        }

        void SignOut()
        {
            Preferences.Remove("apitoken");

            SetMainPage();
        }

        protected override void OnStart()
        {
            AppCenter.Start("ios={Your iOS app secret here};"
               + "android={Your Android app secret here};"
               + "uwp={Your UWP app secret here}",
               typeof(Analytics), typeof(Crashes));
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
