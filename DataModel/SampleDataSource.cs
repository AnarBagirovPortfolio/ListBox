using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.DataTransfer;

namespace ListBox.Data
{
    //Класс продукта
    public class SampleDataItem
    {
        public SampleDataItem(string uniqueId, string title, double count, double price, bool ischecked, string description)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Count = count;
            this.Price = price;
            this.isChecked = ischecked;
            this.Description = description;
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public double Count { get; set; }
        public double Price { get; set; }
        public bool isChecked { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    //Класс списка
    public class SampleDataGroup
    {
        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public ObservableCollection<SampleDataItem> Items { get; private set; }

        public SampleDataGroup(string uniqueId, string title, string subtitle)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Items = new ObservableCollection<SampleDataItem>();
        }

        public SampleDataGroup(JsonObject jObject)
        {
            this.UniqueId = jObject.GetNamedString("UniqueId", "");
            this.Title = jObject.GetNamedString("Title", "");
            this.Subtitle = jObject.GetNamedString("SubTitle", "");
            this.Items = new ObservableCollection<SampleDataItem>();

            foreach (IJsonValue item in jObject.GetNamedArray("SampleDataItemArray", new JsonArray()))
            {
                this.Items.Add(new SampleDataItem(item.GetObject().GetNamedString("UniqueId", ""),
                    item.GetObject().GetNamedString("Title", ""),
                    item.GetObject().GetNamedNumber("Count"),
                    item.GetObject().GetNamedNumber("Price"),
                    item.GetObject().GetNamedBoolean("isChecked"),
                    item.GetObject().GetNamedString("Description")));
            }
        }

        public SampleDataGroup(string jObject)
        {
            JsonObject newObject = JsonObject.Parse(jObject);

            this.UniqueId = newObject.GetNamedString("UniqueId", "");
            this.Title = newObject.GetNamedString("Title", "");
            this.Subtitle = newObject.GetNamedString("SubTitle", "");
            this.Items = new ObservableCollection<SampleDataItem>();

            foreach (IJsonValue item in newObject.GetNamedArray("SampleDataItemArray", new JsonArray()))
            {
                this.Items.Add(new SampleDataItem(item.GetObject().GetNamedString("UniqueId", ""),
                    item.GetObject().GetNamedString("Title", ""),
                    item.GetObject().GetNamedNumber("Count"),
                    item.GetObject().GetNamedNumber("Price"),
                    item.GetObject().GetNamedBoolean("isChecked"),
                    item.GetObject().GetNamedString("Description")));
            }
        }

        public override string ToString()
        {
            return this.Title;
        }

        //Количество не купленных продуктов в списке
        public int GetUncheckedItemsCount
        {
            get
            {
                return Items.Count(n => n.isChecked == false);
            }
        }

        //Суммарная стоимость списка
        public string GetSummaryPrice
        {
            get
            {
                return App.resourceLoader.GetString("TotalPrice") + " " + 
                    Math.Round(Items.Select(n => n.Count * n.Price).Sum(), 2).ToString();
            }
        }
    }

    public sealed class SampleDataSource
    {
        private static SampleDataSource _sampleDataSource = new SampleDataSource();

        private ObservableCollection<SampleDataGroup> _groups = new ObservableCollection<SampleDataGroup>();
        public ObservableCollection<SampleDataGroup> Groups
        {
            get { return this._groups; }
        }

        //Получение данных
        private async Task GetSampleDataAsync()
        {
            if (this._groups.Count != 0)
                return;

            string LocalData = await GetJSONStringAsync();

            if (LocalData != null && LocalData != "SomeString")
            {
                try
                {
                    foreach (SampleDataGroup group in GetGroupsFromJsonString(LocalData))
                    {
                        this.Groups.Add(group);
                    }
                }
                catch
                {

                }
            }
        }

        //Получение списков
        public static async Task<IEnumerable<SampleDataGroup>> GetGroupsAsync()
        {
            await _sampleDataSource.GetSampleDataAsync();

            return _sampleDataSource.Groups;
        }

        //Получение списка
        public static async Task<SampleDataGroup> GetGroupAsync(string uniqueId)
        {
            await _sampleDataSource.GetSampleDataAsync();
            var matches = _sampleDataSource.Groups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1)
            {
                App.GroupIndex = _sampleDataSource.Groups.IndexOf(matches.First());
                return matches.First();
            }
            return null;
        }

        //Получение продукта
        public static async Task<SampleDataItem> GetItemAsync(string uniqueId)
        {
            await _sampleDataSource.GetSampleDataAsync();
            var matches = _sampleDataSource.Groups[App.GroupIndex].Items.Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1)
            {
                App.ItemIndex = _sampleDataSource.Groups[App.GroupIndex].Items.IndexOf(matches.First());
                return matches.First();
            }
            return null;
        }

        //Добавление списка
        public static async Task AddGroup(string InputString)
        {
            string GroupSubtitle = App.resourceLoader.GetString("Date") + " " + DateTime.Now.ToString("g");

            //Проверка наличия списка с такиим же именем
            var matches = _sampleDataSource.Groups.Where((group) => group.UniqueId.ToLower().Equals(InputString.Trim().ToLower()));

            if (matches.Count() == 0)
            {
                _sampleDataSource.Groups.Add(new SampleDataGroup(InputString.Trim(), InputString.Trim(), GroupSubtitle));
            }
            else
            {
                await new MessageDialog(App.resourceLoader.GetString("AddGroupError")).ShowAsync();
            }
        }

        //Добавление продукта
        public static async Task AddItem(string InputString)
        {
            //Проверка наличия в списке продукта с такиим же именем
            var matches = _sampleDataSource.Groups[App.GroupIndex].Items.Where((item) => item.UniqueId.ToLower().Equals(InputString.Trim().ToLower()));

            if (matches.Count() == 0)
            {
                _sampleDataSource.Groups[App.GroupIndex].Items.Add(new SampleDataItem(InputString.Trim(), InputString.Trim(), 1, 1, false, ""));
            }
            else
            {
                await new MessageDialog(App.resourceLoader.GetString("AddItemError")).ShowAsync();
            }
        }

        //Удаление купленных продуктов
        public static async Task DeleteCheckedItems()
        {
            for (int i = _sampleDataSource.Groups[App.GroupIndex].Items.Count - 1; i >= 0; i--)
            {
                if (_sampleDataSource.Groups[App.GroupIndex].Items[i].isChecked == true)
                {
                    _sampleDataSource.Groups[App.GroupIndex].Items.RemoveAt(i);
                }
            }

            await WriteJSONStringAsync();
        }

        //Очиска списка
        public static async Task ClearList()
        {
            for (int i = _sampleDataSource.Groups[App.GroupIndex].Items.Count - 1; i >= 0; i--)
            {
                _sampleDataSource.Groups[App.GroupIndex].Items.RemoveAt(i);
            }

            await WriteJSONStringAsync();
        }

        //Удаление списка
        public static async Task DeleteGroup()
        {
            _sampleDataSource.Groups.RemoveAt(App.GroupIndex);

            await WriteJSONStringAsync();
        }

        public static async Task DeleteGroup(string uniqueId)
        {
            var Group = await SampleDataSource.GetGroupAsync(uniqueId);

            _sampleDataSource.Groups.Remove(Group);

            await WriteJSONStringAsync();
        }

        //Удаление продукта
        public static async Task DeleteItem()
        {
            _sampleDataSource.Groups[App.GroupIndex].Items.RemoveAt(App.ItemIndex);

            await WriteJSONStringAsync();
        }

        //Получение суммарной стоимости текущего списка
        public static string GroupSummaryPrice()
        {
            return _sampleDataSource.Groups[App.GroupIndex].GetSummaryPrice;
        }

        //Получение количества не купленных продуктов текущего списка
        public static string GroupUncheckedItemsCount()
        {
            return _sampleDataSource.Groups[App.GroupIndex].GetUncheckedItemsCount.ToString();
        }

        //Сортировка продуктов в списке
        public static async Task SortItems()
        {
            await _sampleDataSource.GetSampleDataAsync();
            /*
            //Получение отсортированного списка
            var SortedItems = _sampleDataSource.Groups[App.GroupIndex].Items.OrderBy(n => n.Title).OrderBy(n => n.isChecked).ToList();

            if (_sampleDataSource.Groups[App.GroupIndex].Items.Count <= 150)
            {
                //Очиска списка
                for (int i = _sampleDataSource.Groups[App.GroupIndex].Items.Count - 1; i >= 0; i--)
                {
                    _sampleDataSource.Groups[App.GroupIndex].Items.RemoveAt(i);

                    //Добавление продукта из отсортированного списка в i
                    _sampleDataSource.Groups[App.GroupIndex].Items.Insert(i, SortedItems[i]);
                }
            }
            else
            {
                //Очиска списка
                for (int i = _sampleDataSource.Groups[App.GroupIndex].Items.Count - 1; i >= 0; i--)
                {
                    _sampleDataSource.Groups[App.GroupIndex].Items.RemoveAt(i);
                }

                //Добавление продуктов из отсортированного списка
                foreach (var item in SortedItems)
                {
                    _sampleDataSource.Groups[App.GroupIndex].Items.Add(item);
                }
            }*/

            for (int i = 1; i < _sampleDataSource.Groups[App.GroupIndex].Items.Count; i++ )
            {
                if (String.Compare(_sampleDataSource.Groups[App.GroupIndex].Items[i].Title, _sampleDataSource.Groups[App.GroupIndex].Items[i-1].Title) < 0)
                {
                    for (int j = 0; j < i; j++)
                    {
                        if (String.Compare(_sampleDataSource.Groups[App.GroupIndex].Items[i].Title, _sampleDataSource.Groups[App.GroupIndex].Items[j].Title) < 0)
                        {
                            _sampleDataSource.Groups[App.GroupIndex].Items.Move(i, j);
                            break;
                        }
                    }
                }
            }

            for (int i = 1; i < _sampleDataSource.Groups[App.GroupIndex].Items.Count; i++)
            {
                if (_sampleDataSource.Groups[App.GroupIndex].Items[i].isChecked.CompareTo(_sampleDataSource.Groups[App.GroupIndex].Items[i - 1].isChecked) < 0)
                {
                    for (int j = 0; j < i; j++)
                    {
                        if (_sampleDataSource.Groups[App.GroupIndex].Items[i].isChecked.CompareTo(_sampleDataSource.Groups[App.GroupIndex].Items[j].isChecked) < 0)
                        {
                            _sampleDataSource.Groups[App.GroupIndex].Items.Move(i, j);
                            break;
                        }
                    }
                }
            }
        }

        //Помечение продукта купленным
        public static void ItemChecked(string uniqueId)
        {
            if (uniqueId != null)
            {
                var matches = _sampleDataSource.Groups[App.GroupIndex].Items.Where((item) => item.UniqueId.Equals(uniqueId));
                App.ItemIndex = _sampleDataSource.Groups[App.GroupIndex].Items.IndexOf(matches.First());
                if (App.ItemIndex != -1)
                {
                    _sampleDataSource.Groups[App.GroupIndex].Items[App.ItemIndex].isChecked = true;
                }
            }            
        }

        //Помечение продукта не купленным
        public static void ItemUnchecked(string uniqueId)
        {
            if (uniqueId != null)
            {
                var matches = _sampleDataSource.Groups[App.GroupIndex].Items.Where((item) => item.UniqueId.Equals(uniqueId));
                App.ItemIndex = _sampleDataSource.Groups[App.GroupIndex].Items.IndexOf(matches.First());
                if (App.ItemIndex != -1)
                {
                    _sampleDataSource.Groups[App.GroupIndex].Items[App.ItemIndex].isChecked = false;
                }
            }    
        }

        //Изменение количества, цены и описания продукта
        public static void ChangeCountAndPrice(double Count, double Price, string Description)
        {
            _sampleDataSource.Groups[App.GroupIndex].Items[App.ItemIndex].Count = Count;
            _sampleDataSource.Groups[App.GroupIndex].Items[App.ItemIndex].Price = Price;
            _sampleDataSource.Groups[App.GroupIndex].Items[App.ItemIndex].Description = Description;
        }

        //Создание строки JSON-сериализации
        public static string CreateJsonString()
        {
            if (_sampleDataSource.Groups.Count != 0)
            {
                JsonArray JsonGroups = new JsonArray();
                JsonObject JsonGroup = new JsonObject();
                JsonArray JsonGroupItems = new JsonArray();
                JsonObject JsonGroupItem = new JsonObject();
                foreach (SampleDataGroup group in _sampleDataSource.Groups)
                {
                    JsonGroup = new JsonObject();
                    JsonGroup["UniqueId"] = JsonValue.CreateStringValue(group.UniqueId);
                    JsonGroup["Title"] = JsonValue.CreateStringValue(group.Title);
                    JsonGroup["SubTitle"] = JsonValue.CreateStringValue(group.Subtitle);

                    JsonGroupItems = new JsonArray();
                    foreach (SampleDataItem item in group.Items)
                    {
                        JsonGroupItem = new JsonObject();
                        JsonGroupItem["UniqueId"] = JsonValue.CreateStringValue(item.UniqueId);
                        JsonGroupItem["Title"] = JsonValue.CreateStringValue(item.Title);
                        JsonGroupItem["Count"] = JsonValue.CreateNumberValue(item.Count);
                        JsonGroupItem["Price"] = JsonValue.CreateNumberValue(item.Price);
                        JsonGroupItem["isChecked"] = JsonValue.CreateBooleanValue(item.isChecked);
                        JsonGroupItem["Description"] = JsonValue.CreateStringValue(item.Description);
                        JsonGroupItems.Add(JsonGroupItem);
                    }

                    JsonGroup["SampleDataItemArray"] = JsonGroupItems;
                    JsonGroups.Add(JsonGroup);
                }

                return JsonGroups.Stringify();
            }
            else
            {
                return "SomeString";
            }
        }

        //Получение списков из JSON-строки
        public ObservableCollection<SampleDataGroup> GetGroupsFromJsonString(string JsonString)
        {
            JsonArray convertedJsonGroups = JsonArray.Parse(JsonString);
            ObservableCollection<SampleDataGroup> groups = new ObservableCollection<SampleDataGroup>();
            foreach (IJsonValue group in convertedJsonGroups)
            {
                groups.Add(new SampleDataGroup(group.GetObject()));
            }

            return groups;
        }

        //Запись информации в локальный файл
        public static async Task WriteJSONStringAsync()
        {
            StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            StorageFile SampleFile = await localFolder.CreateFileAsync("data.txt", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(SampleFile, CreateJsonString());
        }

        //Получение строки из локального файла
        public async Task<string> GetJSONStringAsync()
        {
            StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            try
            {
                StorageFile localFile = await localFolder.GetFileAsync("data.txt");

                return await FileIO.ReadTextAsync(localFile);
            }
            catch
            {
                return null;
            }
        }

        public static string CreateMessage(SampleDataGroup someGroup)
        {
            return someGroup.Title + ": " + String.Join("; ", someGroup.Items.Select(n => n.Title + " - " + n.Count).ToArray());
        }

        public class ShareText
        {
            private static ShareText _shareText = new ShareText();

            static int SendOption = 0;

            private void RegisterForShare()
            {
                DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
                dataTransferManager.DataRequested += new TypedEventHandler<DataTransferManager,
                    DataRequestedEventArgs>(this.ShareTextHandler);
            }

            private void ShareTextHandler(DataTransferManager sender, DataRequestedEventArgs e)
            {
                GetShareText(e.Request);
            }

            private void GetShareText(DataRequest request)
            {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                DataPackage requestData = request.Data;
                requestData.Properties.Title = _sampleDataSource.Groups[App.GroupIndex].Title;
                if (SendOption == 0)
                {
                    if (_sampleDataSource.Groups[App.GroupIndex].Items.Count != 0)
                    {
                        requestData.SetText(String.Join("\n", _sampleDataSource.Groups[App.GroupIndex].Items.Select(n => n.Title + ": " + loader.GetString("Count") + " - " + n.Count + ", " + loader.GetString("Price") + " - " + n.Price + ", " + loader.GetString("TotalPrice") + " - " + Math.Round(n.Count * n.Price, 2)).ToArray())
                            + "\n\n" + _sampleDataSource.Groups[App.GroupIndex].GetSummaryPrice);
                    }
                    else
                    {
                        requestData.SetText(" ");
                    }
                }

                else if (SendOption == 1)
                {
                    if (_sampleDataSource.Groups[App.GroupIndex].Items.Where(n => n.isChecked == false).ToList().Count != 0)
                    {
                        //requestData.SetText(String.Join("\n", _sampleDataSource.Groups[App.GroupIndex].Items.Where(n => n.isChecked == false).Select(n => n.Title + ":  " + loader.GetString("Count") + " - " + n.Count + ", " + loader.GetString("Price") + " - " + n.Price + ", " + loader.GetString("TotalPrice") + " - " + Math.Round(n.Count * n.Price, 2)).ToArray())
                        //    + "\n\n" + loader.GetString("TotalPrice") + " " + Math.Round(_sampleDataSource.Groups[App.GroupIndex].Items.Where(n => n.isChecked == false).Select(n => n.Count * n.Price).Sum(), 2));
                        requestData.SetText(String.Join(", ", _sampleDataSource.Groups[App.GroupIndex].Items.Where(n => n.isChecked == false).Select(n => n.Title).ToArray()));
                    }
                    else
                    {
                        requestData.SetText(" ");
                    }
                }

                else
                {
                    if (_sampleDataSource.Groups[App.GroupIndex].Items.Count != 0)
                    {
                        requestData.SetText(String.Join(", ", _sampleDataSource.Groups[App.GroupIndex].Items.Select(n => n.Title).ToArray()));
                    }
                    else
                    {
                        requestData.SetText(" ");
                    }
                }
            }

            public static void Share(int sendoption)
            {
                SendOption = sendoption;
                _shareText.RegisterForShare();
                DataTransferManager.ShowShareUI();
            }
        }
    }
}