﻿using System;

using Xamarin.Forms;

using XamList.Shared;
using XamList.Mobile.Shared;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace XamList
{
    public class ContactsListPage : BaseContentPage<ContactsListViewModel>
    {
        #region Constant Fields
        Xamarin.Forms.ListView _contactsListView;
        #endregion

        #region Constructors
        public ContactsListPage()
        {
            var addContactButton = new ToolbarItem
            {
                Text = "+",
                AutomationId = AutomationIdConstants.AddContactButon
            };
            addContactButton.Clicked += HandleAddContactButtonClicked;
            ToolbarItems.Add(addContactButton);

            _contactsListView = new Xamarin.Forms.ListView(ListViewCachingStrategy.RecycleElement)
            {
                ItemTemplate = new DataTemplate(typeof(ContactsListTextCell)),
                IsPullToRefreshEnabled = true,
                BackgroundColor = Color.Transparent,
                AutomationId = AutomationIdConstants.ContactsListView
            };
            _contactsListView.ItemSelected += HandleItemSelected;
            _contactsListView.SetBinding(ItemsView<Cell>.ItemsSourceProperty, nameof(ViewModel.AllContactsList));
            _contactsListView.SetBinding(Xamarin.Forms.ListView.RefreshCommandProperty, nameof(ViewModel.RefreshCommand));
            _contactsListView.SetBinding(Xamarin.Forms.ListView.IsRefreshingProperty, nameof(ViewModel.IsRefreshing));

            var restoreDeletedContactsButton = new Button
            {
                Text = "  Restore Deleted Contacts  ",
                TextColor = ColorConstants.TextColor,
                AutomationId = AutomationIdConstants.RestoreDeletedContactsButton,
                BackgroundColor = new Color(ColorConstants.NavigationBarBackgroundColor.R,
                                            ColorConstants.NavigationBarBackgroundColor.G,
                                            ColorConstants.NavigationBarBackgroundColor.B,
                                            0.25)
            };
            restoreDeletedContactsButton.Clicked += HandleRestoreDeletedContactsButtonClicked;

            Title = PageTitleConstants.ContactsListPage;

            var relativeLayout = new RelativeLayout();

            Func<RelativeLayout, double> getRestoreDeletedContactsButtonHeight = parent => restoreDeletedContactsButton.Measure(parent.Width, parent.Height).Request.Height;
            Func<RelativeLayout, double> getRestoreDeletedContactsButtonWidth = parent => restoreDeletedContactsButton.Measure(parent.Width, parent.Height).Request.Width;

            relativeLayout.Children.Add(_contactsListView,
                                       Constraint.Constant(0),
                                       Constraint.Constant(0),
                                       Constraint.RelativeToParent(parent => parent.Width),
                                       Constraint.RelativeToParent(parent => parent.Height));
            relativeLayout.Children.Add(restoreDeletedContactsButton,
                                        Constraint.RelativeToParent(parent => parent.Width / 2 - getRestoreDeletedContactsButtonWidth(parent) / 2),
                                        Constraint.RelativeToParent(parent => parent.Height - getRestoreDeletedContactsButtonHeight(parent) - 10));

            Content = relativeLayout;

            On<iOS>().SetUseSafeArea(true);
        }
        #endregion

        #region Methods
        protected override void OnAppearing()
        {
            base.OnAppearing();

            AppCenterHelpers.TrackEvent(AppCenterConstants.ContactsListPageAppeared);

            Device.BeginInvokeOnMainThread(_contactsListView.BeginRefresh);
        }

        void HandleItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var listView = sender as Xamarin.Forms.ListView;
            var selectedContactModel = e?.SelectedItem as ContactModel;

            Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PushAsync(new ContactDetailPage(selectedContactModel, false));
                listView.SelectedItem = null;
            });
        }

        void HandleAddContactButtonClicked(object sender, EventArgs e)
        {
            AppCenterHelpers.TrackEvent(AppCenterConstants.AddContactButtonTapped);

            Device.BeginInvokeOnMainThread(async () =>
               await Navigation.PushModalAsync(new BaseNavigationPage(new ContactDetailPage(new ContactModel(), true))));
        }

        void HandleRestoreDeletedContactsButtonClicked(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                var isDisplayAlertDialogConfirmed = await DisplayAlert("Restore Deleted Contacts",
                                                            "Would you like to restore deleted contacts?",
                                                            AlertDialogConstants.Yes,
                                                            AlertDialogConstants.Cancel);

                if (isDisplayAlertDialogConfirmed)
                    ViewModel.RestoreDeletedContactsCommand?.Execute(null);
            });
        }
        #endregion
    }
}
