// Copyright (c) 2025 PinePie. All rights reserved.

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PinePie.PieTabs
{
    public interface IButtonData
    {
        string Label { get; }

        VisualElement GetButtonElement(VisualTreeAsset template, Action<VisualElement> onClick);
    }

    public class PieButton<T> where T : IButtonData
    {
        public T buttonProp;
        public VisualElement UIbutton;

        public PieButton(T buttonProp, VisualElement button)
        {
            this.buttonProp = buttonProp;
            UIbutton = button;
        }
    }

    // abstract base class for both buttons
    public abstract class ButtonBundle<TButton> where TButton : IButtonData
    {
        public List<PieButton<TButton>> buttons = new();


        public void AddButton(TButton button, VisualTreeAsset template)
        {
            var btn = new PieButton<TButton>(button, button.GetButtonElement(template, DepSelBehaviour));

            buttons.Add(btn);

            SaveToJson();
        }

        public void InsertAt(int index, TButton button, VisualTreeAsset template)
        {
            var btn = new PieButton<TButton>(button, button.GetButtonElement(template, DepSelBehaviour));

            buttons.Insert(index, btn);

            SaveToJson();
        }

        public void RemoveButton(TButton toRemove)
        {
            int index = buttons.FindIndex(b => EqualityComparer<TButton>.Default.Equals(b.buttonProp, toRemove));
            if (index >= 0)
            {
                buttons.RemoveAt(index);
                SaveToJson();
            }
        }

        public void LoadFromJson(VisualTreeAsset template)
        {
            buttons = LoadButtonData()
                .Select(b => new { data = b, element = b.GetButtonElement(template, DepSelBehaviour) })
                .Where(pair => pair.element != null)
                .Select(pair => new PieButton<TButton>(pair.data, pair.element))
                .ToList();
        }

        public void DepSelBehaviour(VisualElement clickedButton)
        {
            foreach (var btn in buttons)
                btn.UIbutton.RemoveFromClassList("selected");

            clickedButton.AddToClassList("selected");
        }

        public abstract void SaveToJson();
        protected abstract List<TButton> LoadButtonData();
    }

    // shortcut button
    [Serializable]
    public class ShortcutButton : IButtonData
    {
        public string label;
        public string guid;
        public bool isMinimal;
        public string color = "#3E3E3E";

        public string Label => label;
        public string Path => AssetDatabase.GUIDToAssetPath(guid);


        public ShortcutButton(string label, string guid, bool isMinimal = false, string color = "#3E3E3E")
        {
            this.label = label;
            this.guid = guid;
            this.isMinimal = isMinimal;
            this.color = color;
        }

        public VisualElement GetButtonElement(VisualTreeAsset template, Action<VisualElement> onClick)
        {
            var button = template.Instantiate().Q<VisualElement>("customButton");
            if (button == null) return null;

            Navigator.SetupButtonProperties(false, button, isMinimal, Label, color);

            // icon
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(Path);
            if (obj == null) return null;

            Texture2D iconTexture = EditorGUIUtility.ObjectContent(obj, obj.GetType()).image as Texture2D;
            button.Q<VisualElement>("buttonIcon").style.backgroundImage = new StyleBackground(iconTexture);

            // shade
            Navigator.SetShadeProperties(button, isMinimal, Label, iconTexture);

            Navigator.OnShortcutButtonClicked(button, onClick, this);
            return button;
        }
    }

    public class ShortcutButtonBundle : ButtonBundle<ShortcutButton>
    {
        public override void SaveToJson() => ButtonCache<ShortcutButton>.Save(buttons.Select(b => b.buttonProp).ToList());

        protected override List<ShortcutButton> LoadButtonData() => ButtonCache<ShortcutButton>.Load();
    }

    // creator button data
    [Serializable]
    public class CreatorButton : IButtonData
    {
        public string menuEntry;
        public string iconName;
        public bool isMinimal = false;
        public string color = "#3E3E3E";

        public string Label => string.IsNullOrEmpty(menuEntry) ? "" : menuEntry[(menuEntry.LastIndexOf('/') + 1)..];


        public CreatorButton(string entry, UnityEngine.Object obj, bool isMinimal = false, string color = "#3E3E3E")
        {
            menuEntry = entry;
            iconName = EditorGUIUtility.ObjectContent(obj, obj.GetType()).image.name;
            this.isMinimal = isMinimal;
            this.color = color;
        }

        public VisualElement GetButtonElement(VisualTreeAsset template, Action<VisualElement> onClick)
        {
            var button = template.Instantiate().Q<VisualElement>("customButton");
            if (button == null) return null;

            Navigator.SetupButtonProperties(true, button, isMinimal, Label, color);

            // icon
            Texture2D iconTexture = EditorGUIUtility.IconContent(iconName).image as Texture2D;
            button.Q<VisualElement>("buttonIcon").style.backgroundImage = new StyleBackground(iconTexture);

            // shade
            Navigator.SetShadeProperties(button, isMinimal, Label, iconTexture);

            Navigator.OnAssetCreatorButtonClicked(button, onClick, this);
            return button;
        }
    }

    public class CreatorButtonBundle : ButtonBundle<CreatorButton>
    {
        public override void SaveToJson() => ButtonCache<CreatorButton>.Save(buttons.Select(b => b.buttonProp).ToList());
        protected override List<CreatorButton> LoadButtonData() => ButtonCache<CreatorButton>.Load();
    }

    public class ClickState
    {
        public bool leftClicked;
        public bool rightClicked;
    }


}

#endif