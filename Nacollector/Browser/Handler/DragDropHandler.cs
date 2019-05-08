using CefSharp;
using CefSharp.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nacollector.Browser.Handler
{
    /**
     * 获取可拖拽区域
     */
    public class DragDropHandler : IDragHandler
    {
        public Region draggableRegion = null;
        public event Action<Region> RegionsChanged;
        public bool Enable { get; set; } = false;

        public bool OnDragEnter(IWebBrowser browserControl, IBrowser browser, IDragData dragData, DragOperationsMask mask)
        {
            return false;
        }

        public void OnDraggableRegionsChanged(IWebBrowser browserControl, IBrowser browser, IList<DraggableRegion> regions)
        {
            if (Enable == false)
            {
                return;
            }

            if (browser.IsPopup == false)
            {
                draggableRegion = null;
                if (regions != null && regions.Count > 0)
                {
                    foreach (var region in regions)
                    {
                        // Console.WriteLine(region.X + " - " + region.Y + " - " + region.Width + " - " + region.Height);
                        var rect = new Rectangle(region.X, region.Y, region.Width, region.Height);

                        if (draggableRegion == null)
                        {
                            draggableRegion = new Region(rect);
                        }
                        else
                        {
                            if (region.Draggable)
                            {
                                draggableRegion.Union(rect);
                            }
                            else
                            {
                                //In the scenario where we have an outer region, that is draggable and it has
                                // an inner region that's not, we must exclude the non draggable.
                                // Not all scenarios are covered in this example.
                                draggableRegion.Exclude(rect);
                            }
                        }
                    }
                }

                RegionsChanged?.Invoke(draggableRegion);
            }
        }

        public void Dispose()
        {
            RegionsChanged = null;
        }
    }
}
