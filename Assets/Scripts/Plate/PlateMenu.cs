using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PlateSpace
{
    public class EditPlateMenu : Menu, Clickable
    {
        public static event Action<EditPlateMenu> Clicked = delegate { };
        Guid plate_id;


        void OnMouseDown()
        {
            Clicked(this);
            Debug.Log("Clicked event sent");
        }

        public void OnHover()
        {

        }

        public void OnSelect()
        {
            SetVisible(true);
        }

        // Start is called before the first frame update
        protected override void Initialise()
        {
            base.Initialise();
            MenuLayer background = AddAnimationLayer<MenuLayer>("background", "plate_menu", Color.black, true, true);
            background.SetScreenSize(10);
            InputFieldLayer name_input = AddAnimationLayer<InputFieldLayer>("plate_name", "text_field", Color.black, true, true);
            name_input.AddSpriteMap("text_field", Color.green);
            name_input.ChangeAnimation(1);
            name_input.SetScreenSize(9f);
            name_input.m_input.onEndEdit.AddListener(field_value =>
            {
                GetAnimationLayer<InputFieldLayer>("plate_name").SetText(Plate.plate[plate_id].name);
            });
            name_input.m_input.onSubmit.AddListener(field_value =>
            {
                ChangeName(field_value);
            });
            SetScreenOffset(background.GetOffset(new Anchor(VAnchor.Bottom, HAnchor.Left)));
        }
        public void UnHover()
        {

        }

        public void UnSelect()
        {
            SetVisible(false);
        }


        public void SetPlate(Plate given_plate)
        {
            plate_id = given_plate.id;
            GetAnimationLayer<InputFieldLayer>("plate_name").SetText(given_plate.name);
        }

        public void ChangeName(string name)
        {
            try
            {
                Plate.plate[plate_id].name = name;
                GetAnimationLayer<InputFieldLayer>("plate_name").SetText(name);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        public void OnSubmit(string name)
        {
            ChangeName(name);
        }

        private void Update()
        {
            Animate();
        }
    }
}
