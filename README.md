# ViewModelBaseExtensions

## Project Description

This is a small base framework for MVVM in winRT,Windows Phone, WPF and Silverlight. Also included some necessary extension methods.
NuGet

Also WinRT (For Windows 8 Store Applciations) version is available at NuGet http://www.nuget.org/packages/VMBase/
Details

As a developer you could build your viewmodel according to the our VMBase and VMPageBase.
VMBase

Inheriting VMBase to your ViewModels of your project will following abilities to your View Models:
- Marshaling : VMBase objects could be marshaled as unmanaged memory bytes and could be read from unmanaged bytes.
- XML : VMBase objects could be serialized to XML bytes and XML Strings and cloud deserialized form XML strings and XML bytes.
- Json : VMBase objects could be serialized to JSON bytes and JSON Strings and cloud deserialized form JSON strings and JSON bytes. To do that Newtonsoft Json.NET has been used.
- Storage: VMBase objects could be save themselves to the local data folder as serialized XML files. Also there are static methods in VMBase could save string and byte arrays in the data folder.

There are also virtual methods Initialize, Suspend and Clone. Those could be override in the Child class and used for registering / unregistering event handlers and resources.

Also INotifyPropertyChanged Interface is properly implemented. With public void NotifyPropertyChanged(string info) method any MVVM property can be notified in the setter.
