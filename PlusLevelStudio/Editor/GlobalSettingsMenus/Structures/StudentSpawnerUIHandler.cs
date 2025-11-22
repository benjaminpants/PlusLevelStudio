using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace PlusLevelStudio.Editor.GlobalSettingsMenus.Structures
{
    public class StudentSpawnerUIHandler : GlobalStructureUIHandler
    {
        public TextMeshProUGUI minStudentText;
        public TextMeshProUGUI maxStudentText;
        public StudentSpawnerStructureLocation studentStructure
        {
            get
            {
                return (structure == null) ? null : (StudentSpawnerStructureLocation)structure;
            }
        }
        public override bool GetStateBoolean(string key)
        {
            return false;
        }

        public override void OnElementsCreated()
        {
            minStudentText = transform.Find("MinStudentsBox").GetComponent<TextMeshProUGUI>();
            maxStudentText = transform.Find("MaxStudentsBox").GetComponent<TextMeshProUGUI>();
        }

        public override void PageLoaded(StructureLocation structure)
        {
            base.PageLoaded(structure);
            minStudentText.text = studentStructure.minStudents.ToString();
            maxStudentText.text = studentStructure.maxStudents.ToString();
        }

        public override void SendInteractionMessage(string message, object data = null)
        {
            switch (message)
            {
                case "minStudentsEnter":
                    ushort.TryParse((string)data, out studentStructure.minStudents);
                    PageLoaded(structure);
                    break;
                case "maxStudentsEnter":
                    ushort.TryParse((string)data, out studentStructure.maxStudents);
                    PageLoaded(structure);
                    break;
            }
        }
    }
}
