// Copyright 2011 Jimmy Schementi
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Schementi.Controls.Utilities {
    public static class DependencyObjectExtensions {
        private static IEnumerable<DependencyObject> GetVisualChildren(DependencyObject depObj) {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++) {
                yield return VisualTreeHelper.GetChild(depObj, i);
            }
        }

        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject depObj) where T : DependencyObject {
            if (depObj == null) yield break;
            foreach (var child in GetVisualChildren(depObj)) {
                if (child != null && child is T)
                    yield return (T)child;
                foreach (var childOfChild in FindVisualChildren<T>(child))
                    yield return childOfChild;
            }
        }
#if SILVERLIGHT
        private static IEnumerable<T> GetItemsControlChildren<T>(this DependencyObject depObj) where T : DependencyObject {
            if (depObj == null || !(depObj is ItemsControl)) return Enumerable.Empty<T>();
            return ((ItemsControl)depObj).Items.ToList().Compact().Where(c => c is T).Cast<T>();
        }
#endif
        public static IEnumerable<T> FindLogicalChildren<T>(this DependencyObject depObj) where T : DependencyObject {
#if SILVERLIGHT
            // Silverlight doesn't have a logical tree, so look at ItemsControl children.
            return depObj == null ? 
                Enumerable.Empty<T>() :
                GetItemsControlChildren<T>(depObj)
                    .Concat(GetVisualChildren(depObj).SelectMany(GetItemsControlChildren<T>));
#else
            if (depObj == null) yield break;
            foreach (var child in LogicalTreeHelper.GetChildren(depObj)) {
                if (child != null && child is T)
                    yield return (T)child;
                var depObjChild = child as DependencyObject;
                if (depObjChild != null)
                    foreach (var childOfChild in FindLogicalChildren<T>(depObjChild))
                        yield return childOfChild;
            }
#endif
        }
    }
}
