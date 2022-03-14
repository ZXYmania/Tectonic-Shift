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
        public List<InputFieldLayer> input_fields;


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
            input_fields = new List<InputFieldLayer>();
            MenuLayer background = AddAnimationLayer<MenuLayer>("background", "plate_menu", Color.black, true, true);
            background.SetScreenSize(10);
            background.AddBoxCollider();
            InputFieldLayer name_input = AddAnimationLayer<InputFieldLayer>("plate_name", "text_field", Color.black, true, true);
            input_fields.Add(name_input);
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
            GetAnimationLayer<InputFieldLayer>("plate_name").Unselect();
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

        public InputFieldLayer GetSelectedTextField()
        {
            for(int i = 0; i< input_fields.Count; i++)
            {
                if(input_fields[i].Selected())
                {
                    return input_fields[i];
                }
            }
            return null;
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
