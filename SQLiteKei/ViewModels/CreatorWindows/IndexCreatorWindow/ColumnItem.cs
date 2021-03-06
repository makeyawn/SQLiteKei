﻿using SQLiteKei.Util;
using SQLiteKei.ViewModels.Base;

using System.Collections.Generic;

namespace SQLiteKei.ViewModels.CreatorWindows.IndexCreatorWindow
{
    public class ColumnItem : NotifyingModel
    {
        public string ColumnName { get; private set; }

        private string selectedAction;
        public string SelectedAction
        {
            get { return selectedAction; }
            set { selectedAction = value;  NotifyPropertyChanged(); }
        }

        public List<string> AvailableActions { get; private set; }

        public ColumnItem(string columnName)
        {
            ColumnName = columnName;

            AvailableActions = new List<string>
            {
                LocalisationHelper.GetString("IndexCreator_ColumnAction_DoNotUse"),
                LocalisationHelper.GetString("IndexCreator_ColumnAction_Ascending"),
                LocalisationHelper.GetString("IndexCreator_ColumnAction_Descending")
            };

            SelectedAction = LocalisationHelper.GetString("IndexCreator_ColumnAction_DoNotUse");
        }
    }
}
