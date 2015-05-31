/*
 * $Id$
 * It is part of the SolidOpt Copyright Policy (see Copyright.txt)
 * For further details see the nearest License.txt
 */

using System;

using SolidOpt.Services;

namespace SolidIDE
{
  /// <summary>
  /// Exposes methods and events an application has to implement in order to be compatible with
  /// the SolidIDE's plugins
  /// </summary>
  /// 
  public interface ISolidIDE : IService
  {
    /// <summary>
    /// Occurs when the main application (SolidIDE) is requested to terminate.
    /// The event is sent to the subscribed plugins allowing them to do some work before the
    /// application is terminated.
    /// </summary>
    event EventHandler<EventArgs> OnShutDown;

    /// <summary>
    /// Gets the current user configuration directory.
    /// </summary>
    /// <description>
    /// On Unix it is /home/&lt;user&gt;/.config/SolidIDE
    /// On Mac it is /Users/&lt;user&gt;/.config/SolidIDE
    /// On Windows it is in AppData
    /// </description>
    /// <returns>
    /// The configuration directory.
    /// </returns>
    ///
    string GetConfigurationDirectory();

    /// <summary>
    /// Gets the application main window.
    /// </summary>
    /// <returns>
    /// The main window.
    /// </returns>
    ///
    //FIXME: Must return Gtk.Window
    MainWindow GetMainWindow();

    /// <summary>
    /// Gets the application main menu.
    /// </summary>
    /// <returns>
    /// The main menu.
    /// </returns>
    /// 
    Gtk.MenuBar GetMainMenu();

    /// <summary>
    /// Gets the application toolbar.
    /// </summary>
    /// <returns>
    /// The toolbar.
    /// </returns>
    /// 
    Gtk.Toolbar GetToolbar();

    /// <summary>
    /// Gets the currently loaded plugins in the application.
    /// </summary>
    /// <returns>
    /// The plugins.
    /// </returns>
    /// 
    PluginServiceContainer GetPlugins();

    /// <summary>
    /// Disable or enable exit from the application.
    /// </summary>
    /// 
    void DisableQuit(bool disable);

    /// <summary>
    /// Gets the service container.
    /// </summary>
    /// <returns>The service container.</returns>
    IServiceContainer GetServiceContainer();

    /// <summary>
    /// Gets the menu or menu item.
    /// </summary>
    /// <returns>The menu item.</returns>
    /// <param name="menuNames">Last param is menu item name, other are sub menu path names.</param>
    /// <typeparam name="MenuItemType">The type of menu item.</typeparam>
    MenuItemType GetMenuItem<MenuItemType>(params string[] menuNames) where MenuItemType : Gtk.MenuItem, new();

    /// <summary>
    /// Gets the toolbar item.
    /// </summary>
    /// <returns>The toolbar item.</returns>
    /// <param name="toolbarNames">Last param is toolbar item name, other are sub toolbar groups.</param>
    /// <typeparam name="ToolbarItemType">The type of toolbar item.</typeparam>
    ToolItemType GetToolbarItem<ToolItemType>(params string[] toolbarNames) where ToolItemType : Gtk.ToolItem, new();

    /// <summary>
    /// Gets the toolbar item.
    /// </summary>
    /// <returns>The toolbar item.</returns>
    /// <param name="newItem">New item.</param>
    /// <param name="toolbarNames">Toolbar names.</param>
    Gtk.ToolItem GetToolbarItem(Gtk.ToolItem newItem, params string[] toolbarNames);
  }
}