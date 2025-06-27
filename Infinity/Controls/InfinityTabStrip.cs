using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DevComponents.DotNetBar;
using DevComponents.DotNetBar.Metro;

namespace NoDev.Infinity.Controls
{
    internal class SelectedTabChangedEventArgs : EventArgs
    {
        internal InfinityTab Tab { get; private set; }

        internal SelectedTabChangedEventArgs(InfinityTab tab)
        {
            Tab = tab;
        }
    }

    [ToolboxItem(false)]
    internal class InfinityTabStrip : MetroTabStrip
    {
        internal InfinityTabCollection Tabs { get; private set; }

        internal event EventHandler<SelectedTabChangedEventArgs> SelectedTabChanged;

        private InfinityTab _selectedTab;

        internal new InfinityTab SelectedTab
        {
            get { return _selectedTab; }
            set
            {
                if (value == _selectedTab)
                    return;

                var tab = Items.OfType<MetroTabItem>().FirstOrDefault(t => (InfinityTab)t.Tag == value);

                if (tab == null)
                    throw new Exception("Tab not present in collection.");

                tab.Select();
            }
        }

        internal InfinityTabStrip()
        {
            Tabs = new InfinityTabCollection(this);
            ButtonCheckedChanged += OnButtonCheckedChangedHandler;
        }

        internal void SelectTabByID(string id)
        {
            if (_selectedTab != null && _selectedTab.ID == id)
                return;

            var tab = Items.OfType<MetroTabItem>().FirstOrDefault(t => ((InfinityTab)t.Tag).ID == id);

            if (tab == null)
                throw new Exception("Tab not present in collection.");

            tab.Select();
        }

        private void OnButtonCheckedChangedHandler(object sender, EventArgs e)
        {
            if (SelectedTabChanged == null)
                return;

            var selectedTab = base.SelectedTab;

            var newSelection = selectedTab != null ? (InfinityTab)selectedTab.Tag : null;

            if (newSelection == _selectedTab)
                return;

            _selectedTab = newSelection;

            SelectedTabChanged(this, new SelectedTabChangedEventArgs(_selectedTab));
        }
    }

    internal class InfinityTab
    {
        internal string ID { get; private set; }
        internal MetroTabItem TabItem { get; set; }

        internal string Text
        {
            get { return TabItem == null ? _text : TabItem.Text; }
            set
            {
                if (TabItem == null)
                {
                    _text = value;
                }
                else
                {
                    TabItem.Text = value;
                }
            }
        }

        private string _text;

        internal InfinityTab(string id, string text)
        {
            ID = id;
            _text = text;
        }
    }

    internal class InfinityTabCollection : ICollection<InfinityTab>
    {
        private readonly InfinityTabStrip _tabStrip;

        internal InfinityTabCollection(InfinityTabStrip tabStrip)
        {
            _tabStrip = tabStrip;
        }

        public IEnumerator<InfinityTab> GetEnumerator()
        {
            return
                _tabStrip.Items.OfType<MetroTabItem>()
                    .Select<MetroTabItem, InfinityTab>(t => (InfinityTab)t.Tag)
                    .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(InfinityTab item)
        {
            var tab = new MetroTabItem
            {
                Text = item.Text, 
                Tag = item
            };

            item.TabItem = tab;

            _tabStrip.Items.Add(tab);
        }

        public void Clear()
        {
            _tabStrip.Items.RemoveRange(_tabStrip.Items.OfType<MetroTabItem>().Cast<BaseItem>().ToArray());
        }

        public bool Contains(InfinityTab item)
        {
            return _tabStrip.Items.OfType<MetroTabItem>().Any(t => ((InfinityTab)t.Tag) == item);
        }

        public void CopyTo(InfinityTab[] array, int arrayIndex)
        {
            var tabs = _tabStrip.Items.OfType<MetroTabItem>()
                .Select<MetroTabItem, InfinityTab>(t => (InfinityTab) t.Tag)
                .ToArray();

            Array.Copy(tabs, 0, array, arrayIndex, tabs.Length);
        }

        public bool Remove(InfinityTab item)
        {
            var tab = _tabStrip.Items.OfType<MetroTabItem>().FirstOrDefault(t => ((InfinityTab)t.Tag) == item);

            if (tab == null)
                return false;

            _tabStrip.Items.Remove(tab);

            return true;
        }

        public int Count
        {
            get { return _tabStrip.Items.OfType<MetroTabItem>().Count(); }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }
    }
}
