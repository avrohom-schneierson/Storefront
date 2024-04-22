﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SemesterProject
{
    public partial class Storefront : Form
    {
        private DataClasses1DataContext db;
        private IEnumerator<STORE_ITEM> AllStoreItems;
        private List<STORE_ITEM> CachedStoreItems = new List<STORE_ITEM>();
        private readonly int LoggedInCustomerId;
        private List<STORE_ITEM> ItemsInCart = new List<STORE_ITEM>();
        private bool IsAnotherItem { get; set; }  // todo naming

        private readonly int NumItemsPerPage = 4;  // todo maybe derive from gui
        private int CurrentPageNum = 0;  // 0-indexed for easy use with collections
        private int CurrentPageNumDisplay  // 1-indexed for user display
        {
            get
            {
                return CurrentPageNum + 1;
            }
        }

        public Storefront(DataClasses1DataContext db, int loggedInCustomerId)
        {
            InitializeComponent();

            this.db = db;
            this.LoggedInCustomerId = loggedInCustomerId;
            //ViewableStoreItems = new STORE_ITEM[NumItemsPerPage];
            LoadCustomerInfo();
            LoadStoreItemsIntoGUI(GetStoreItems(CurrentPageNum));
            // todo should next page button be disabled if less than 4 items in store? else will throw error
            if (IsAnotherItem)
            {
                btnNextPage.Enabled = true;
            }
        }

        /// <summary>
        /// Go through each of the storeItems and populate each GUI listing with the item's details.
        /// </summary>
        /// <param name="storeItems">The items to populate the GUI listings with</param>
        private void LoadStoreItemsIntoGUI(IEnumerable<STORE_ITEM> storeItems)
        {
            // current implementation of this method circles around and overwrites listings if more storeItems contains more than NumItemsPerPage
            // todo should we circle around though?
            int i = 0;
            foreach (STORE_ITEM storeItem in storeItems)
            {
                StoreItemListing sil = new StoreItemListing(storeItem);
                Panel listing = pnlAllListings.Controls["pnlListing" + i] as Panel;
                // ((PictureBox) listing.Controls["pbxItemImage" + i]) todo set
                listing.Controls["rtbMainItemInfo" + i].Text = sil.Title;
                listing.Controls["rtbMinorItemInfo" + i].Text = sil.FormattedPrice;
                (listing.Controls["nudQuantity" + i] as NumericUpDown).Maximum = sil.StoreItem.QuantityAvailable;

                i = (i + 1) % NumItemsPerPage;  // move to next listing to update, reset to the first listing (index 0) if we move past the last listing
            }
        }

        private void AddItemToCart(object sender, EventArgs e)
        {
            string pattern = "\\w+(\\d+)";
            string buttonName = (sender as Button).Name;
            int itemIndex;
            Match match = Regex.Match(buttonName, pattern);
            if (match.Success)
            {
                itemIndex = Convert.ToInt32(match.Groups[1].Value);

                // TODO UPTO 4/21/24;

                // todo do gui acknowledgment of add to cart with a timer so it goes back to normal:
                (sender as Button).BackColor = Color.Green;
                (sender as Button).ForeColor = Color.White;


                // todo add item to user's cart so it can be purchased on cart tab
                pnlAllListings.Controls["pnlListing" + itemIndex].Controls["rtbMainItemInfo" + itemIndex].Text += "\n\nItem added to cart";
                ItemsInCart.Add(CachedStoreItems[(CurrentPageNum * NumItemsPerPage) + itemIndex]);
                dataGridView1.DataSource = ItemsInCart;  // todo this doesn't refresh, look into how to do this data bindign etc.

                // todo look into bug of multiple add to cart lines added when add to cart after using next page button multiple times
            }

            
            
            // todo look into dat bindings for the GUI controls
            // STORE_ITEM item = ((sender as Button).Parent as Panel).DataBindings[0].DataSource as STORE_ITEM;
        }

        /// <summary>
        /// Get the store items for the given page number. 
        /// </summary>
        /// <param name="pageNum">The page number to retrieve items for</param>
        /// <returns>An IEnumerable of the store items for the page</returns>
        private IEnumerable<STORE_ITEM> GetStoreItems(int pageNum)
        {
            if (AllStoreItems == null)
            {
                RetrieveAllStoreItems();
                IsAnotherItem = AllStoreItems.MoveNext();
            }

            // get any items from cache before retrieving from the db
            int i = 0;
            bool desiredItemIsCached = (pageNum * NumItemsPerPage + i) < CachedStoreItems.Count;
            while (desiredItemIsCached && i < NumItemsPerPage)
            {
                yield return CachedStoreItems[pageNum * NumItemsPerPage + i];
                i++;
                desiredItemIsCached = (pageNum * NumItemsPerPage + i) < CachedStoreItems.Count;
            }

            // retrieve the rest of the items from the db (unless we already retrieved all items
            // (i.e., i == NumItemsPerPage), and/or until there are no more items left)
            for (; i < NumItemsPerPage && IsAnotherItem; i++)
            {
                STORE_ITEM storeItem = AllStoreItems.Current;
                CachedStoreItems.Add(storeItem);
                yield return storeItem;
                IsAnotherItem = AllStoreItems.MoveNext();
            }
        }

        private void RetrieveAllStoreItems()
        {
            // The order retrieved here will be the order of the items as displayed to the user
            // todo use a smarter ordering maybe? not just based on quantity
            AllStoreItems = db.STORE_ITEMs.OrderByDescending(item => item.QuantityAvailable).GetEnumerator();
        }

        private void LoadCustomerInfo()
        {
            // todo
        }

        private void btnNextPage_Click(object sender, EventArgs e)
        {
            CurrentPageNum++;
            UpdateLblPageNum();
            LoadStoreItemsIntoGUI(GetStoreItems(CurrentPageNum));

            btnPreviousPage.Enabled = true;
            bool IsNoMoreCachedItems = CurrentPageNum >= CachedStoreItems.Count / NumItemsPerPage;
            if (IsNoMoreCachedItems && !IsAnotherItem)
            {
                btnNextPage.Enabled = false;
            }
        }

        private void btnPreviousPage_Click(object sender, EventArgs e)
        {
            CurrentPageNum--;
            UpdateLblPageNum();
            LoadStoreItemsIntoGUI(GetStoreItems(CurrentPageNum));

            btnNextPage.Enabled = true;
            if (CurrentPageNum == 0)
            {
                btnPreviousPage.Enabled = false;
            }
        }

        private void UpdateLblPageNum()
        {
            lblPageNum.Text = "Page " + CurrentPageNumDisplay;
        }


        private void Storefront_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'storeDB_Purchases2.PURCHASE' table. You can move, or remove it, as needed.
            //this.pURCHASETableAdapter1.Fill(db.PURCHASEs.Where(row => row.CustomerId == LoggedInCustomerId);
        }
    }
}
