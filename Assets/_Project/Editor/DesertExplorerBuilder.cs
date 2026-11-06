using System.Collections.Generic;
using UnityEngine;

namespace DesertRush.Editor
{
    public static partial class DesertAssetGenerator
    {
        private static RunnerMotor Explorer(out Material[] outfits)
        {
            outfits = new[] { Surface("Scout indigo", 0x245663), Surface("Sentinel moss", 0x697D45),
                Surface("Cartographer carmine", 0xA24641) };
            var cloth = Surface("Trail canvas",0xE8CAA2);
            var leather = Surface("Explorer leather",0x5A372C);
            var skin = Surface("Explorer skin",0xB7744C);
            var root = new GameObject("Sahir the courier");
            var collider = root.AddComponent<CharacterController>();
            collider.radius = .32f;
            collider.height = RunnerMotor.NormalHeight;
            collider.center = V(0,RunnerMotor.NormalHeight/2,0);
            collider.stepOffset = .16f;
            collider.skinWidth = .035f;
            collider.minMoveDistance = 0f;
            var motor = root.AddComponent<RunnerMotor>();
            var pose = root.AddComponent<ExplorerPose>();
            var changeable = new List<Renderer>();
            pose.hips = Bone("Pelvis",root.transform,V(0,.94f,0));
            pose.chest = Bone("Spine",pose.hips,V(0,.18f,0));
            pose.head = Bone("Head",pose.chest,V(0,.43f,0));
            pose.scarf = Bone("Scarf tail",pose.chest,V(.13f,.35f,-.14f));
            changeable.Add(Garment("Travel tunic",pose.chest,outfits[0],
                Ring(-.19f,.21f,.136f),Ring(-.10f,.225f,.147f),Ring(.16f,.275f,.155f),
                Ring(.28f,.313f,.158f),Ring(.37f,.184f,.118f)));
            Garment("Leather belt",pose.hips,leather,Ring(.065f,.223f,.15f),Ring(.14f,.238f,.16f));
            Garment("Neck wrap",pose.chest,cloth,Ring(.306f,.19f,.145f),Ring(.385f,.184f,.157f),Ring(.421f,.136f,.13f));
            Garment("Face",pose.head,skin,Ring(-.053f,.066f,.086f),Ring(.02f,.122f,.112f),
                Ring(.145f,.145f,.124f),Ring(.24f,.105f,.095f));
            var headGear = new DesertMesh(3);
            headGear.Lathe(0,Vector3.zero,Quaternion.identity,12,Ring(.119f,.151f,.134f),
                Ring(.167f,.168f,.143f),Ring(.248f,.146f,.132f),Ring(.29f,.09f,.086f));
            headGear.Beam(1,V(0,.113f,.11f),V(.222f,.062f,.072f),.026f);
            headGear.Beam(2,V(-.055f,.122f,.15f),V(.073f,.032f,.016f),.012f);
            headGear.Beam(2,V(.055f,.122f,.15f),V(.073f,.032f,.016f),.012f);
            headGear.Beam(0,V(0,.047f,-.115f),V(.25f,.24f,.072f),.026f);
            Part(Model("Woven headwrap","Characters",headGear,cloth,leather,metal),pose.head);
            var kit = new DesertMesh(3);
            kit.Beam(0,V(0,.13f,-.215f),V(.35f,.41f,.21f),.077f);
            kit.Beam(1,V(0,.193f,-.322f),V(.31f,.24f,.055f),.038f);
            kit.Lathe(1,V(0,.357f,-.26f),Quaternion.Euler(0,0,90),10,
                Ring(-.26f,.073f,.073f),Ring(-.217f,.087f,.087f),Ring(.217f,.087f,.087f),Ring(.26f,.072f,.072f));
            for (int s=-1;s<=1;s+=2)
            {
                kit.Beam(0,V(s*.133f,.151f,.146f),V(.057f,.37f,.045f),Quaternion.Euler(0,0,s*9f));
                kit.Beam(2,V(s*.132f,.07f,.177f),V(.072f,.06f,.031f));
            }
            Part(Model("Field pack and harness","Characters",kit,leather,cloth,metal),pose.chest);
            var scarf = new DesertMesh(1);
            for (int i=0;i<6;i++)
            {
                var a = V(.03f*Mathf.Sin(i*.4f),-.022f*i,-.105f*i);
                var b = V(.03f*Mathf.Sin((i+1)*.4f),-.022f*(i+1),-.105f*(i+1));
                float w = .074f-i*.007f;
                scarf.Face(0,a-Vector3.right*w,b-Vector3.right*w,b+Vector3.right*w,a+Vector3.right*w);
                scarf.Face(0,a+Vector3.right*w,b+Vector3.right*w,b-Vector3.right*w,a-Vector3.right*w);
            }
            Part(Model("Trailing sash","Characters",scarf,cloth),pose.scarf);
            for (int s=-1;s<=1;s+=2)
            {
                string label = s<0 ? "Left" : "Right";
                Transform leg = Bone(label+" thigh",pose.hips,V(s*.139f,-.01f,0));
                Transform shin = Bone(label+" shin",leg,V(0,-.43f,0));
                Transform arm = Bone(label+" upper arm",pose.chest,V(s*.271f,.287f,0));
                Transform forearm = Bone(label+" forearm",arm,V(0,-.298f,0));
                Garment(label+" trousers",leg,cloth,Ring(-.44f,.097f,.093f),Ring(-.27f,.12f,.113f),
                    Ring(-.04f,.147f,.134f),Ring(.064f,.112f,.109f));
                Garment(label+" gaiter",shin,leather,Ring(-.39f,.081f,.082f),Ring(-.31f,.105f,.104f),
                    Ring(-.16f,.105f,.104f),Ring(-.04f,.083f,.086f));
                var boot = new DesertMesh(2);
                boot.Beam(0,V(0,-.378f,.059f),V(.182f,.129f,.298f),.047f);
                boot.Beam(1,V(0,-.441f,.061f),V(.193f,.045f,.311f),.043f);
                Part(Model(label+" desert boot","Characters",boot,leather,metal),shin);
                changeable.Add(Garment(label+" sleeve",arm,outfits[0],Ring(-.31f,.074f,.071f),
                    Ring(-.18f,.098f,.086f),Ring(-.013f,.119f,.101f),Ring(.05f,.083f,.089f)));
                Garment(label+" gauntlet",forearm,leather,Ring(-.24f,.05f,.053f),
                    Ring(-.17f,.078f,.074f),Ring(-.04f,.086f,.075f),Ring(.009f,.063f,.065f));
                Garment(label+" hand",forearm,skin,Ring(-.316f,.034f,.048f),
                    Ring(-.265f,.053f,.060f),Ring(-.215f,.05f,.049f));
                if (s<0) { pose.leftLeg=leg; pose.leftShin=shin; pose.leftArm=arm; pose.leftForearm=forearm; }
                else { pose.rightLeg=leg; pose.rightShin=shin; pose.rightArm=arm; pose.rightForearm=forearm; }
            }
            pose.outfitPanels = changeable.ToArray();
            return SavePrefab(root,"Player").GetComponent<RunnerMotor>();
        }

        private static Transform Bone(string name, Transform parent, Vector3 position)
        {
            var bone = new GameObject(name).transform;
            bone.SetParent(parent,false);
            bone.localPosition = position;
            return bone;
        }

        private static void Part(GameObject part, Transform bone) => part.transform.SetParent(bone,false);

        private static Renderer Garment(string name, Transform bone, Material material, params Vector3[] rings)
        {
            var mesh = new DesertMesh(1);
            mesh.Lathe(0,Vector3.zero,Quaternion.identity,12,rings);
            var model = Model(name,"Characters",mesh,material);
            Part(model,bone);
            return model.GetComponent<Renderer>();
        }
    }
}
