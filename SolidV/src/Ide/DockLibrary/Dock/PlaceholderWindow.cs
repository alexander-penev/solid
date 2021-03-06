/*
 * $Id$
 * It is part of the SolidOpt Copyright Policy (see Copyright.txt)
 * For further details see the nearest License.txt
 */
using System;
using Gdk;
using Gtk;

namespace SolidV.Ide.Dock
{
  using Gtk = global::Gtk;
  internal class PlaceholderWindow: Gtk.Window
  {
    Gdk.GC redgc;
    uint anim;
    int rx, ry, rw, rh;
    bool allowDocking;
    
    public bool AllowDocking {
      get {
        return allowDocking;
      }
      set {
        allowDocking = value;
      }
    }
    
    public PlaceholderWindow (DockFrame frame): base (Gtk.WindowType.Popup)
    {
      SkipTaskbarHint = true;
      Decorated = false;
      TransientFor = (Gtk.Window) frame.Toplevel;
      TypeHint = WindowTypeHint.Utility;
      
      // Create the mask for the arrow
      
      Realize ();
      redgc = new Gdk.GC (GdkWindow);
         redgc.RgbFgColor = frame.Style.Background (StateType.Selected);
    }
    
    void CreateShape (int width, int height)
    {
      Gdk.Color black, white;
      black = new Gdk.Color (0, 0, 0);
      black.Pixel = 1;
      white = new Gdk.Color (255, 255, 255);
      white.Pixel = 0;
      
      Gdk.Pixmap pm = new Pixmap (this.GdkWindow, width, height, 1);
      Gdk.GC gc = new Gdk.GC (pm);
      gc.Background = white;
      gc.Foreground = white;
      pm.DrawRectangle (gc, true, 0, 0, width, height);
      
      gc.Foreground = black;
      pm.DrawRectangle (gc, false, 0, 0, width - 1, height - 1);
      pm.DrawRectangle (gc, false, 1, 1, width - 3, height - 3);
      
      this.ShapeCombineMask (pm, 0, 0);
    }
    
    protected override void OnSizeAllocated (Rectangle allocation)
    {
      base.OnSizeAllocated (allocation);
      CreateShape (allocation.Width, allocation.Height);
    }

    
    protected override bool OnExposeEvent (Gdk.EventExpose args)
    {
      //base.OnExposeEvent (args);
      int w, h;
      this.GetSize (out w, out h);
      this.GdkWindow.DrawRectangle (redgc, false, 0, 0, w-1, h-1);
      this.GdkWindow.DrawRectangle (redgc, false, 1, 1, w-3, h-3);
        return true;
    }
    
    public void Relocate (int x, int y, int w, int h, bool animate)
    {
      if (x != rx || y != ry || w != rw || h != rh) {
        Move (x, y);
        Resize (w, h);
        
        rx = x; ry = y; rw = w; rh = h;
        
        if (anim != 0) {
          GLib.Source.Remove (anim);
          anim = 0;
        }
        if (animate && w < 150 && h < 150) {
          int sa = 7;
          Move (rx-sa, ry-sa);
          Resize (rw+sa*2, rh+sa*2);
          anim = GLib.Timeout.Add (10, RunAnimation);
        }
      }
    }
    
    bool RunAnimation ()
    {
      int cx, cy, ch, cw;
      GetSize (out cw, out ch);
      GetPosition  (out cx, out cy);
      
      if (cx != rx) {
        cx++; cy++;
        ch-=2; cw-=2;
        Move (cx, cy);
        Resize (cw, ch);
        return true;
      }
      anim = 0;
      return false;
    }
  }
}
