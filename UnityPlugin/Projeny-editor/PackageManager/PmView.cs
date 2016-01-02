using System;
using System.IO;
using UnityEditorInternal;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Projeny.Internal;
using System.Linq;

namespace Projeny.Internal
{
    public enum ListTypes
    {
        Package,
        Release,
        AssetItem,
        PluginItem,
        Count
    }

    public class ContextMenuItem
    {
        public readonly bool IsEnabled;
        public readonly string Caption;
        public readonly Action Handler;
        public readonly bool IsChecked;

        public ContextMenuItem(
            bool isEnabled, string caption, bool isChecked, Action handler)
        {
            IsEnabled = isEnabled;
            Caption = caption;
            Handler = handler;
            IsChecked = isChecked;
        }
    }

    public class PmView
    {
        const string NotAvailableLabel = "N/A";

        public event Action ViewStateChanged = delegate {};
        public event Action<ProjectConfigTypes> ClickedProjectType = delegate {};
        public event Action ClickedRefreshReleaseList = delegate {};
        public event Action ClickedRefreshPackages = delegate {};
        public event Action ClickedCreateNewPackage = delegate {};
        public event Action ClickedProjectApplyButton = delegate {};
        public event Action ClickedProjectRevertButton = delegate {};
        public event Action ClickedProjectSaveButton = delegate {};
        public event Action ClickedProjectEditButton = delegate {};
        public event Action<Rect> ClickedReleasesSortMenu = delegate {};
        public event Action<DraggableList.DragData, DraggableList> DragDroppedListItem = delegate {};

        public event Action<ListTypes, ListTypes, List<DraggableListEntry>> DraggedDroppedListEntries = delegate {};

        readonly Dictionary<ListTypes, Func<IEnumerable<ContextMenuItem>>> _contextMenuHandlers = new Dictionary<ListTypes, Func<IEnumerable<ContextMenuItem>>>();

        readonly List<PopupInfo> _popupHandlers = new List<PopupInfo>();

        readonly List<DraggableList> _lists = new List<DraggableList>();

        readonly Model _model;

        float _split1 = 0;
        float _split2 = 0.5f;

        float _lastTime = 0.5f;

        bool _doesConfigFileExist = true;

        int _popupIdCount;

        PackageManagerWindowSkin _skin;

        public PmView(Model model)
        {
            _model = model;

            for (int i = 0; i < (int)ListTypes.Count; i++)
            {
                var list = new DraggableList(
                    this, (ListTypes)i, _model.ListModels[i]);

                _lists.Add(list);
            }
        }

        public PmViewStates ViewState
        {
            get
            {
                return _model.ViewState;
            }
            set
            {
                if (_model.ViewState != value)
                {
                    _model.ViewState = value;
                    ViewStateChanged();
                }
            }
        }

        public ProjectConfigTypes ProjectConfigType
        {
            get
            {
                return _model.ProjectConfigType;
            }
            set
            {
                _model.ProjectConfigType = value;
            }
        }

        public ReleasesSortMethod ReleasesSortMethod
        {
            get
            {
                return _model.ReleasesSortMethod;
            }
            set
            {
                _model.ReleasesSortMethod = value;
            }
        }

        public bool ReleaseSortAscending
        {
            get
            {
                return _model.ReleaseSortAscending;
            }
            set
            {
                _model.ReleaseSortAscending = value;
            }
        }

        public string BlockedStatusMessage
        {
            get;
            set;
        }

        public string BlockedStatusTitle
        {
            get;
            set;
        }

        public void AddContextMenuHandler(ListTypes listType, Func<IEnumerable<ContextMenuItem>> handler)
        {
            _contextMenuHandlers.Add(listType, handler);
        }

        public void RemoveContextMenuHandler(ListTypes listType)
        {
            _contextMenuHandlers.RemoveWithConfirm(listType);
        }

        object GetReleaseSortField(DraggableListEntry entry)
        {
            Assert.Throw("TODO");
            return entry.Name;

            //var info = (ReleaseInfo)entry.Tag;

            //switch (_model.ReleasesSortMethod)
            //{
                //case ReleasesSortMethod.Name:
                //{
                    //return info.Name;
                //}
                //case ReleasesSortMethod.Size:
                //{
                    //return info.CompressedSize;
                //}
                //case ReleasesSortMethod.PublishDate:
                //{
                    //return info.AssetStoreInfo == null ? 0 : info.AssetStoreInfo.PublishDateTicks;
                //}
            //}

            //Assert.Throw();
            //return null;
        }

        public List<DraggableListEntry> GetSelected(ListTypes listType)
        {
            return _lists[(int)listType].GetSelected();
        }

        public void ClearOtherListSelected(ListTypes type)
        {
            foreach (var list in _lists)
            {
                if (list.ListType != type)
                {
                    list.ClearSelected();
                }
            }
        }

        public void ClearSelected()
        {
            foreach (var list in _lists)
            {
                list.ClearSelected();
            }
        }

        public List<DraggableListEntry> SortList(DraggableList list, List<DraggableListEntry> entries)
        {
            return entries.OrderBy(x => x.Name).ToList();
            //switch (list.ListType)
            //{
                //case ListTypes.Release:
                //{
                    //if (_model.ReleaseSortAscending)
                    //{
                        //return entries.OrderBy(x => GetReleaseSortField(x)).ToList();
                    //}

                    //return entries.OrderByDescending(x => GetReleaseSortField(x)).ToList();
                //}
                //default:
                //{
                    //return entries.OrderBy(x => x.Name).ToList();
                //}
            //}
        }

        public void DrawItemLabel(Rect rect, DraggableListEntry entry)
        {
            Assert.Throw("TODO");
            //DrawListItem(rect, entry.Name);

            //switch (entry.ListOwner.ListType)
            //{
                //case ListTypes.Release:
                //{
                    //var info = (ReleaseInfo)(entry.Tag);

                    //var labelStr = info.Name;

                    //if (_model.IsReleaseInstalled(info))
                    //{
                        //labelStr = ImguiUtil.WrapWithColor(labelStr, Skin.Theme.DraggableItemAlreadyAddedColor);
                    //}

                    //DrawItemLabelWithVersion(rect, labelStr, info.Version);
                    //break;
                //}
                //case ListTypes.Package:
                //{
                //}
                //case ListTypes.AssetItem:
                //case ListTypes.PluginItem:
                //{
                //}
                //default:
                //{
                    //Assert.Throw();
                    //break;
                //}
            //}
        }

        public PackageManagerWindowSkin Skin
        {
            get
            {
                return _skin ?? (_skin = Resources.Load<PackageManagerWindowSkin>("Projeny/PackageManagerSkin"));
            }
        }

        public bool ShowBlockedPopup
        {
            get;
            set;
        }

        public bool IsBlocked
        {
            get;
            set;
        }

        public void SetListItems(
            ListTypes listType, List<ListItemData> items)
        {
            GetList(listType).SetItems(items);
        }

        DraggableList GetList(ListTypes listType)
        {
            return _lists[(int)listType];
        }

        public bool IsDragAllowed(DraggableList.DragData data, DraggableList list)
        {
            var sourceListType = data.SourceList.ListType;
            var dropListType = list.ListType;

            if (sourceListType == dropListType)
            {
                return true;
            }

            switch (dropListType)
            {
                case ListTypes.Package:
                {
                    return sourceListType == ListTypes.Release || sourceListType == ListTypes.AssetItem || sourceListType == ListTypes.PluginItem;
                }
                case ListTypes.Release:
                {
                    return false;
                }
                case ListTypes.AssetItem:
                {
                    return sourceListType == ListTypes.Package || sourceListType == ListTypes.PluginItem;
                }
                case ListTypes.PluginItem:
                {
                    return sourceListType == ListTypes.Package || sourceListType == ListTypes.AssetItem;
                }
            }

            Assert.Throw();
            return true;
        }

        public void Update()
        {
            var deltaTime = Time.realtimeSinceStartup - _lastTime;
            _lastTime = Time.realtimeSinceStartup;

            var px = Mathf.Clamp(deltaTime * Skin.InterpSpeed, 0, 1);

            _split1 = Mathf.Lerp(_split1, GetDesiredSplit1(), px);
            _split2 = Mathf.Lerp(_split2, GetDesiredSplit2(), px);
        }

        float GetDesiredSplit1()
        {
            if (ViewState == PmViewStates.ReleasesAndPackages)
            {
                return 0.5f;
            }

            return 0;
        }

        void DrawPopupCommon(Rect fullRect, Rect popupRect)
        {
            ImguiUtil.DrawColoredQuad(popupRect, Skin.Theme.LoadingOverlapPopupColor);
        }

        string[] GetConfigTypesDisplayValues()
        {
            return new[]
            {
                ProjenyEditorUtil.ProjectConfigFileName,
                ProjenyEditorUtil.ProjectConfigUserFileName,
                ProjenyEditorUtil.ProjectConfigFileName + " (global)",
                ProjenyEditorUtil.ProjectConfigUserFileName + " (global)",
            };
        }

        public void OnDragDrop(DraggableList.DragData data, DraggableList dropList)
        {
            if (data.SourceList == dropList || !IsDragAllowed(data, dropList))
            {
                return;
            }

            var sourceListType = data.SourceList.ListType;
            var dropListType = dropList.ListType;

            DraggedDroppedListEntries(sourceListType, dropListType, data.Entries);
        }

        public void OpenContextMenu(DraggableList dropList)
        {
            var itemGetter = _contextMenuHandlers.TryGetValue(dropList.ListType);

            if (itemGetter == null)
            {
                return;
            }

            GenericMenu contextMenu = new GenericMenu();

            foreach (var item in itemGetter())
            {
                var handler = item.Handler;
                contextMenu.AddOptionalItem(
                    item.IsEnabled, new GUIContent(item.Caption), item.IsChecked, () => handler());
            }

            contextMenu.ShowAsContext();
        }

        void DrawFileDropdown(Rect rect)
        {
            var dropDownRect = Rect.MinMaxRect(
                rect.xMin,
                rect.yMin,
                rect.xMax - Skin.FileButtonsPercentWidth * rect.width,
                rect.yMax);

            var displayValues = GetConfigTypesDisplayValues();
            var desiredConfigType = (ProjectConfigTypes)EditorGUI.Popup(dropDownRect, (int)_model.ProjectConfigType, displayValues, Skin.DropdownTextStyle);

            GUI.Button(dropDownRect, displayValues[(int)desiredConfigType]);

            if (desiredConfigType != _model.ProjectConfigType)
            {
                ClickedProjectType(desiredConfigType);
            }

            GUI.DrawTexture(new Rect(dropDownRect.xMax - Skin.ArrowSize.x + Skin.ArrowOffset.x, dropDownRect.yMin + Skin.ArrowOffset.y, Skin.ArrowSize.x, Skin.ArrowSize.y), Skin.FileDropdownArrow);

            var startX = rect.xMax - Skin.FileButtonsPercentWidth * rect.width;
            var startY = rect.yMin;
            var endX = rect.xMax;
            var endY = rect.yMax;

            var buttonPadding = Skin.FileButtonsPadding;
            var buttonWidth = ((endX - startX) - 3 * buttonPadding) / 3.0f;
            var buttonHeight = endY - startY;

            startX = startX + buttonPadding;

            bool wasEnabled;
            wasEnabled = GUI.enabled;
            GUI.enabled = _doesConfigFileExist;
            if (GUI.Button(new Rect(startX, startY, buttonWidth, buttonHeight), "Revert"))
            {
                ClickedProjectRevertButton();
            }
            GUI.enabled = wasEnabled;

            startX = startX + buttonWidth + buttonPadding;

            if (GUI.Button(new Rect(startX, startY, buttonWidth, buttonHeight), "Save"))
            {
                ClickedProjectSaveButton();
            }

            startX = startX + buttonWidth + buttonPadding;

            wasEnabled = GUI.enabled;
            GUI.enabled = _doesConfigFileExist;
            if (GUI.Button(new Rect(startX, startY, buttonWidth, buttonHeight), "Edit"))
            {
                ClickedProjectEditButton();
            }
            GUI.enabled = wasEnabled;
        }

        public IEnumerator AlertUser(string message, string title = null)
        {
            return PromptForUserChoice(message, new[] { "Ok" }, title);
        }

        public IEnumerator<int> PromptForUserChoice(string question, string[] choices, string title = null, string styleOverride = null)
        {
            return CoRoutine.Wrap<int>(
                PromptForUserChoiceInternal(question, choices, title, styleOverride));
        }

        public int AddPopup(Action<Rect> handler)
        {
            _popupIdCount++;
            int newId = _popupIdCount;

            Assert.That(_popupHandlers.Where(x => x.Id == newId).IsEmpty());

            _popupHandlers.Add(new PopupInfo(newId, handler));
            return newId;
        }

        public void RemovePopup(int id)
        {
            var info = _popupHandlers.Where(x => x.Id == id).Single();
            _popupHandlers.RemoveWithConfirm(info);
        }

        public IEnumerator PromptForUserChoiceInternal(
            string question, string[] choices, string title = null, string styleOverride = null)
        {
            int choice = -1;

            var skin = Skin.GenericPromptDialog;

            var popupId = AddPopup(delegate(Rect fullRect)
            {
                GUILayout.BeginArea(fullRect);
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.BeginVertical(skin.BackgroundStyle, GUILayout.Width(skin.PopupWidth));
                        {
                            GUILayout.Space(skin.PanelPadding);

                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Space(skin.PanelPadding);

                                GUILayout.BeginVertical();
                                {
                                    if (title != null)
                                    {
                                        GUILayout.Label(title, skin.TitleStyle);
                                        GUILayout.Space(skin.TitleBottomPadding);
                                    }

                                    GUILayout.Label(question, styleOverride == null ? skin.LabelStyle : GUI.skin.GetStyle(styleOverride));

                                    GUILayout.Space(skin.ButtonTopPadding);

                                    GUILayout.BeginHorizontal();
                                    {
                                        GUILayout.FlexibleSpace();

                                        for (int i = 0; i < choices.Length; i++)
                                        {
                                            if (i > 0)
                                            {
                                                GUILayout.Space(skin.ButtonSpacing);
                                            }

                                            if (GUILayout.Button(choices[i], GUILayout.Width(skin.ButtonWidth)))
                                            {
                                                choice = i;
                                            }
                                        }

                                        GUILayout.FlexibleSpace();
                                    }
                                    GUILayout.EndHorizontal();
                                }
                                GUILayout.EndVertical();
                                GUILayout.Space(skin.PanelPadding);
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.Space(skin.PanelPadding);
                        }
                        GUILayout.EndVertical();
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndArea();
            });

            while (choice == -1)
            {
                yield return null;
            }

            RemovePopup(popupId);

            yield return choice;
        }

        public IEnumerator<string> PromptForInput(string label, string defaultValue)
        {
            string userInput = defaultValue;
            InputDialogStates state = InputDialogStates.None;

            bool isFirst = true;

            var popupId = AddPopup(delegate(Rect fullRect)
            {
                if (Event.current.type == EventType.KeyDown)
                {
                    switch (Event.current.keyCode)
                    {
                        case KeyCode.Return:
                        {
                            state = InputDialogStates.Submitted;
                            break;
                        }
                        case KeyCode.Escape:
                        {
                            state = InputDialogStates.Cancelled;
                            break;
                        }
                    }
                }

                var popupRect = ImguiUtil.CenterRectInRect(fullRect, Skin.InputDialog.PopupSize);

                DrawPopupCommon(fullRect, popupRect);

                var contentRect = ImguiUtil.CreateContentRectWithPadding(
                    popupRect, Skin.InputDialog.PanelPadding);

                GUILayout.BeginArea(contentRect);
                {
                    GUILayout.Label(label, Skin.InputDialog.LabelStyle);

                    GUI.SetNextControlName("PopupTextField");
                    userInput = GUILayout.TextField(userInput, 100);
                    GUI.SetNextControlName("");

                    GUILayout.Space(5);

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button("Submit", GUILayout.MaxWidth(100)))
                        {
                            state = InputDialogStates.Submitted;
                        }

                        if (GUILayout.Button("Cancel", GUILayout.MaxWidth(100)))
                        {
                            state = InputDialogStates.Cancelled;
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndArea();

                if (isFirst)
                {
                    isFirst = false;
                    // Need to remove focus then regain focus on the text box for it to select the whole contents
                    GUI.FocusControl("");
                }
                else if (string.IsNullOrEmpty(GUI.GetNameOfFocusedControl()))
                {
                    GUI.FocusControl("PopupTextField");
                }
            });

            while (state == InputDialogStates.None)
            {
                yield return null;
            }

            RemovePopup(popupId);

            if (state == InputDialogStates.Submitted)
            {
                yield return userInput;
            }
            else
            {
                // Just return null
            }
        }

        void DrawMoreInfoRow(PackageManagerWindowSkin.ReleaseInfoMoreInfoDialogProperties skin, string label, string value)
        {
            GUILayout.BeginHorizontal();
            {
                if (value == NotAvailableLabel)
                {
                    GUI.color = skin.NotAvailableColor;
                }
                GUILayout.Label(label + ":", skin.LabelStyle, GUILayout.Width(skin.LabelColumnWidth));
                GUILayout.Space(skin.ColumnSpacing);
                GUILayout.Label(value, skin.ValueStyle, GUILayout.Width(skin.ValueColumnWidth));
                GUI.color = Color.white;
            }
            GUILayout.EndHorizontal();
        }

        IEnumerator OpenMoreInfoPopup(ReleaseInfo info)
        {
            bool isDone = false;

            var skin = Skin.ReleaseMoreInfoDialog;
            Vector2 scrollPos = Vector2.zero;

            var popupId = AddPopup(delegate(Rect fullRect)
            {
                var popupRect = ImguiUtil.CenterRectInRect(fullRect, skin.PopupSize);

                DrawPopupCommon(fullRect, popupRect);

                var contentRect = ImguiUtil.CreateContentRectWithPadding(
                    popupRect, skin.PanelPadding);

                GUILayout.BeginArea(contentRect);
                {
                    GUILayout.Label("Release Info", skin.HeadingStyle);

                    GUILayout.Space(skin.HeadingBottomPadding);

                    scrollPos = GUILayout.BeginScrollView(scrollPos, false, true, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, skin.ScrollViewStyle, GUILayout.Height(skin.ListHeight));
                    {
                        GUILayout.Space(skin.ListPaddingTop);

                        DrawMoreInfoRow(skin, "Name", info.Name);
                        GUILayout.Space(skin.RowSpacing);
                        DrawMoreInfoRow(skin, "Version", string.IsNullOrEmpty(info.Version) ? NotAvailableLabel : info.Version);
                        GUILayout.Space(skin.RowSpacing);
                        DrawMoreInfoRow(skin, "Publish Date", info.AssetStoreInfo != null && !string.IsNullOrEmpty(info.AssetStoreInfo.PublishDate) ? info.AssetStoreInfo.PublishDate : NotAvailableLabel);
                        GUILayout.Space(skin.RowSpacing);
                        DrawMoreInfoRow(skin, "Compressed Size", info.HasCompressedSize ? MiscUtil.ConvertByteSizeToDisplayValue(info.CompressedSize) : NotAvailableLabel);
                        GUILayout.Space(skin.RowSpacing);
                        DrawMoreInfoRow(skin, "Publisher", info.AssetStoreInfo != null && !string.IsNullOrEmpty(info.AssetStoreInfo.PublisherLabel) ? info.AssetStoreInfo.PublisherLabel : NotAvailableLabel);
                        GUILayout.Space(skin.RowSpacing);
                        DrawMoreInfoRow(skin, "Category", info.AssetStoreInfo != null && !string.IsNullOrEmpty(info.AssetStoreInfo.CategoryLabel) ? info.AssetStoreInfo.CategoryLabel : NotAvailableLabel);
                        GUILayout.Space(skin.RowSpacing);
                        DrawMoreInfoRow(skin, "Description", info.AssetStoreInfo != null && !string.IsNullOrEmpty(info.AssetStoreInfo.Description) ? info.AssetStoreInfo.Description : NotAvailableLabel);
                        GUILayout.Space(skin.RowSpacing);
                        DrawMoreInfoRow(skin, "Unity Version", info.AssetStoreInfo != null && !string.IsNullOrEmpty(info.AssetStoreInfo.UnityVersion) ? info.AssetStoreInfo.UnityVersion : NotAvailableLabel);
                        GUILayout.Space(skin.RowSpacing);
                        DrawMoreInfoRow(skin, "ID", info.Id);
                        GUILayout.Space(skin.RowSpacing);
                        DrawMoreInfoRow(skin, "Publish Notes", info.AssetStoreInfo != null && !string.IsNullOrEmpty(info.AssetStoreInfo.PublishNotes) ? info.AssetStoreInfo.PublishNotes : NotAvailableLabel);
                        GUILayout.Space(skin.RowSpacing);
                        DrawMoreInfoRow(skin, "Version Code", info.HasVersionCode ? info.VersionCode.ToString() : NotAvailableLabel);
                        GUILayout.Space(skin.RowSpacing);
                    }
                    GUI.EndScrollView();
                }
                GUILayout.EndArea();

                var okButtonRect = new Rect(
                    contentRect.xMin + 0.5f * contentRect.width - 0.5f * skin.OkButtonWidth,
                    contentRect.yMax - skin.MarginBottom - skin.OkButtonHeight,
                    skin.OkButtonWidth,
                    skin.OkButtonHeight);

                if (GUI.Button(okButtonRect, "Ok") || Event.current.keyCode == KeyCode.Escape)
                {
                    isDone = true;
                }
            });

            while (!isDone)
            {
                yield return null;
            }

            RemovePopup(popupId);
        }

        float GetDesiredSplit2()
        {
            if (ViewState == PmViewStates.ReleasesAndPackages)
            {
                return 1.0f;
            }

            if (ViewState == PmViewStates.Project)
            {
                return 0;
            }

            return 0.4f;
        }

        public void OnGUI(Rect fullRect)
        {
            GUI.skin = Skin.GUISkin;

            if (IsBlocked)
            {
                // Do not allow any input processing when running an async task
                GUI.enabled = false;
            }

            DrawArrowColumns(fullRect);

            var windowRect = Rect.MinMaxRect(
                Skin.ListVerticalSpacing + Skin.ArrowWidth,
                Skin.MarginTop,
                fullRect.width - Skin.ListVerticalSpacing - Skin.ArrowWidth,
                fullRect.height - Skin.MarginBottom);

            if (_split2 >= 0.1f)
            {
                DrawPackagesPane(windowRect);
            }

            if (_split2 <= 0.92f)
            {
                DrawProjectPane(windowRect);
            }

            if (_split1 >= 0.1f)
            {
                DrawReleasePane(windowRect);
            }

            GUI.enabled = true;

            if (IsBlocked)
            {
                if (ShowBlockedPopup || !_popupHandlers.IsEmpty())
                {
                    ImguiUtil.DrawColoredQuad(fullRect, Skin.Theme.LoadingOverlayColor);

                    if (_popupHandlers.IsEmpty())
                    {
                        DisplayGenericProcessingDialog(fullRect);
                    }
                    else
                    {
                        foreach (var info in _popupHandlers)
                        {
                            info.Handler(fullRect);
                        }
                    }
                }
            }
        }

        void DisplayGenericProcessingDialog(Rect fullRect)
        {
            var skin = Skin.AsyncPopupPane;
            var popupRect = ImguiUtil.CenterRectInRect(fullRect, skin.PopupSize);

            DrawPopupCommon(fullRect, popupRect);

            var contentRect = ImguiUtil.CreateContentRectWithPadding(
                popupRect, skin.PanelPadding);

            GUILayout.BeginArea(contentRect);
            {
                string title;

                if (string.IsNullOrEmpty(BlockedStatusTitle))
                {
                    title = "Processing";
                }
                else
                {
                    title = BlockedStatusTitle;
                }

                GUILayout.Label(title, skin.HeadingTextStyle, GUILayout.ExpandWidth(true));
                GUILayout.Space(skin.HeadingBottomPadding);

                string statusMessage = "";

                if (!string.IsNullOrEmpty(BlockedStatusMessage))
                {
                    statusMessage = BlockedStatusMessage;

                    int numExtraDots = (int)(Time.realtimeSinceStartup * skin.DotRepeatRate) % 4;

                    statusMessage += new String('.', numExtraDots);

                    // This is very hacky but the only way I can figure out how to keep the message a fixed length
                    // so that the text doesn't jump around as the number of dots change
                    // I tried using spaces instead of _ but that didn't work
                    statusMessage += ImguiUtil.WrapWithColor(new String('_', 3 - numExtraDots), Skin.Theme.LoadingOverlapPopupColor);
                }

                GUILayout.Label(statusMessage, skin.StatusMessageTextStyle, GUILayout.ExpandWidth(true));
            }

            GUILayout.EndArea();
        }

        void DrawArrowColumns(Rect fullRect)
        {
            var halfHeight = 0.5f * fullRect.height;

            var rect1 = new Rect(
                Skin.ListVerticalSpacing, halfHeight - 0.5f * Skin.ArrowHeight, Skin.ArrowWidth, Skin.ArrowHeight);

            if ((int)ViewState > 0)
            {
                if (GUI.Button(rect1, ""))
                {
                    ViewState = (PmViewStates)((int)ViewState - 1);
                }

                if (Skin.ArrowLeftTexture != null)
                {
                    GUI.DrawTexture(new Rect(rect1.xMin + 0.5f * rect1.width - 0.5f * Skin.ArrowButtonIconWidth, rect1.yMin + 0.5f * rect1.height - 0.5f * Skin.ArrowButtonIconHeight, Skin.ArrowButtonIconWidth, Skin.ArrowButtonIconHeight), Skin.ArrowLeftTexture);
                }
            }

            var rect2 = new Rect(fullRect.xMax - Skin.ListVerticalSpacing - Skin.ArrowWidth, halfHeight - 0.5f * Skin.ArrowHeight, Skin.ArrowWidth, Skin.ArrowHeight);

            var numValues = Enum.GetValues(typeof(PmViewStates)).Length;

            if ((int)ViewState < numValues-1)
            {
                if (GUI.Button(rect2, ""))
                {
                    ViewState = (PmViewStates)((int)ViewState + 1);
                }

                if (Skin.ArrowRightTexture != null)
                {
                    GUI.DrawTexture(new Rect(rect2.xMin + 0.5f * rect2.width - 0.5f * Skin.ArrowButtonIconWidth, rect2.yMin + 0.5f * rect2.height - 0.5f * Skin.ArrowButtonIconHeight, Skin.ArrowButtonIconWidth, Skin.ArrowButtonIconHeight), Skin.ArrowRightTexture);
                }
            }
        }

        void DrawReleasePane(Rect windowRect)
        {
            var startX = windowRect.xMin;
            var endX = windowRect.xMin + _split1 * windowRect.width - Skin.ListVerticalSpacing;
            var startY = windowRect.yMin;
            var endY = windowRect.yMax;

            DrawReleasePane2(Rect.MinMaxRect(startX, startY, endX, endY));
        }

        void DrawReleasePane2(Rect rect)
        {
            var startX = rect.xMin;
            var endX = rect.xMax;
            var startY = rect.yMin;
            var endY = startY + Skin.HeaderHeight;

            GUI.Label(Rect.MinMaxRect(startX, startY, endX, endY), "Releases", Skin.HeaderTextStyle);

            var skin = Skin.ReleasesPane;

            startY = endY;
            endY = startY + skin.IconRowHeight;

            var iconRowRect = Rect.MinMaxRect(startX, startY, endX, endY);
            DrawSearchPane(iconRowRect);

            startY = endY;
            endY = rect.yMax - Skin.ApplyButtonHeight - Skin.ApplyButtonTopPadding;

            GetList(ListTypes.Release).Draw(Rect.MinMaxRect(startX, startY, endX, endY));

            startY = endY + Skin.ApplyButtonTopPadding;
            endY = rect.yMax;

            if (GUI.Button(Rect.MinMaxRect(startX, startY, endX, endY), "Refresh"))
            {
                ClickedRefreshReleaseList();
            }
        }

        void DrawSearchPane(Rect rect)
        {
            var startX = rect.xMin;
            var endX = rect.xMax;
            var startY = rect.yMin;
            var endY = rect.yMax;

            var skin = Skin.ReleasesPane;

            ImguiUtil.DrawColoredQuad(rect, skin.IconRowBackgroundColor);

            endX = rect.xMax - 2 * skin.ButtonWidth;

            var searchBarRect = Rect.MinMaxRect(startX, startY, endX, endY);
            if (searchBarRect.Contains(Event.current.mousePosition))
            {
                ImguiUtil.DrawColoredQuad(searchBarRect, skin.MouseOverBackgroundColor);
            }

            GUI.Label(new Rect(startX + skin.SearchIconOffset.x, startY + skin.SearchIconOffset.y, skin.SearchIconSize.x, skin.SearchIconSize.y), skin.SearchIcon);

            var releaseList = GetList(ListTypes.Release);

            releaseList.SearchFilter = GUI.TextField(
                searchBarRect, releaseList.SearchFilter, skin.SearchTextStyle);

            startX = endX;
            endX = startX + skin.ButtonWidth;

            Rect buttonRect;

            buttonRect = Rect.MinMaxRect(startX, startY, endX, endY);
            if (buttonRect.Contains(Event.current.mousePosition))
            {
                ImguiUtil.DrawColoredQuad(buttonRect, skin.MouseOverBackgroundColor);

                if (Event.current.type == EventType.MouseDown)
                {
                    Assert.Throw("TODO");
                    //_model.ReleaseSortAscending = !_model.ReleaseSortAscending;
                    releaseList.UpdateIndices();
                }
            }
            //GUI.DrawTexture(buttonRect, _model.ReleaseSortAscending ? skin.SortDirDownIcon : skin.SortDirUpIcon);

            startX = endX;
            endX = startX + skin.ButtonWidth;

            buttonRect = Rect.MinMaxRect(startX, startY, endX, endY);
            if (buttonRect.Contains(Event.current.mousePosition))
            {
                ImguiUtil.DrawColoredQuad(buttonRect, skin.MouseOverBackgroundColor);

                if (Event.current.type == EventType.MouseDown)
                {
                    ClickedReleasesSortMenu(buttonRect);
                }
            }
            GUI.DrawTexture(buttonRect, skin.SortIcon);
        }

        void DrawProjectPane(Rect windowRect)
        {
            var startX = windowRect.xMin + _split2 * windowRect.width + Skin.ListVerticalSpacing;
            var endX = windowRect.xMax - Skin.ListVerticalSpacing;
            var startY = windowRect.yMin;
            var endY = windowRect.yMax;

            var rect = Rect.MinMaxRect(startX, startY, endX, endY);

            DrawProjectPane2(rect);
        }

        void DrawProjectPane2(Rect rect)
        {
            var startX = rect.xMin;
            var endX = rect.xMax;
            var startY = rect.yMin;
            var endY = startY + Skin.HeaderHeight;

            GUI.Label(Rect.MinMaxRect(startX, startY, endX, endY), "Project", Skin.HeaderTextStyle);

            startY = endY;
            endY = startY + Skin.FileDropdownHeight;

            DrawFileDropdown(Rect.MinMaxRect(startX, startY, endX, endY));

            startY = endY;
            endY = startY + Skin.HeaderHeight;

            GUI.Label(Rect.MinMaxRect(startX, startY, endX, endY), "Assets Folder", Skin.HeaderTextStyle);

            startY = endY;
            endY = rect.yMax - Skin.ApplyButtonHeight - Skin.ApplyButtonTopPadding;

            DrawProjectPane3(Rect.MinMaxRect(startX, startY, endX, endY));

            startY = endY + Skin.ApplyButtonTopPadding;
            endY = rect.yMax;

            DrawProjectButtons(Rect.MinMaxRect(startX, startY, endX, endY));
        }

        void DrawProjectPane3(Rect listRect)
        {
            var halfHeight = 0.5f * listRect.height;

            var rect1 = new Rect(listRect.x, listRect.y, listRect.width, halfHeight - 0.5f * Skin.ListHorizontalSpacing);
            var rect2 = new Rect(listRect.x, listRect.y + halfHeight + 0.5f * Skin.ListHorizontalSpacing, listRect.width, listRect.height - halfHeight - 0.5f * Skin.ListHorizontalSpacing);

            GetList(ListTypes.AssetItem).Draw(rect1);
            GetList(ListTypes.PluginItem).Draw(rect2);

            GUI.Label(Rect.MinMaxRect(rect1.xMin, rect1.yMax, rect1.xMax, rect2.yMin), "Plugins Folder", Skin.HeaderTextStyle);
        }

        void DrawPackagesPane(Rect windowRect)
        {
            var startX = windowRect.xMin + _split1 * windowRect.width + Skin.ListVerticalSpacing;
            var endX = windowRect.xMin + _split2 * windowRect.width - Skin.ListVerticalSpacing;
            var startY = windowRect.yMin;
            var endY = windowRect.yMax;

            DrawPackagesPane2(Rect.MinMaxRect(startX, startY, endX, endY));
        }

        void DrawPackagesPane2(Rect rect)
        {
            var startX = rect.xMin;
            var endX = rect.xMax;
            var startY = rect.yMin;
            var endY = startY + Skin.HeaderHeight;

            GUI.Label(Rect.MinMaxRect(startX, startY, endX, endY), "Packages", Skin.HeaderTextStyle);

            startY = endY;
            endY = rect.yMax - Skin.ApplyButtonHeight - Skin.ApplyButtonTopPadding;

            GetList(ListTypes.Package).Draw(Rect.MinMaxRect(startX, startY, endX, endY));

            startY = endY + Skin.ApplyButtonTopPadding;
            endY = rect.yMax;

            var horizMiddle = 0.5f * (rect.xMax + rect.xMin);

            endX = horizMiddle - 0.5f * Skin.PackagesPane.ButtonPadding;

            if (GUI.Button(Rect.MinMaxRect(startX, startY, endX, endY), "Refresh"))
            {
                ClickedRefreshPackages();
            }

            startX = endX + Skin.PackagesPane.ButtonPadding;
            endX = rect.xMax;

            if (GUI.Button(Rect.MinMaxRect(startX, startY, endX, endY), "New"))
            {
                ClickedCreateNewPackage();
            }
        }

        void DrawProjectButtons(Rect rect)
        {
            var halfWidth = rect.width * 0.5f;
            var padding = 0.5f * Skin.ProjectButtonsPadding;

            if (GUI.Button(Rect.MinMaxRect(rect.x + halfWidth + padding, rect.y, rect.xMax, rect.yMax), "Apply"))
            {
                ClickedProjectApplyButton();
            }
        }

        enum InputDialogStates
        {
            None,
            Cancelled,
            Submitted
        }

        class PopupInfo
        {
            public readonly int Id;
            public readonly Action<Rect> Handler;

            public PopupInfo(int id, Action<Rect> handler)
            {
                Id = id;
                Handler = handler;
            }
        }

        // View data that needs to be saved and restored
        [Serializable]
        public class Model
        {
            public PmViewStates ViewState = PmViewStates.PackagesAndProject;
            public ProjectConfigTypes ProjectConfigType = ProjectConfigTypes.LocalProject;
            public ReleasesSortMethod ReleasesSortMethod;
            public bool ReleaseSortAscending;
            public List<DraggableList.Model> ListModels = new List<DraggableList.Model>();
        }
    }
}
