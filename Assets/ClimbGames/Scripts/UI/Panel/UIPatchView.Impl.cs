using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using ClimbGames.UI;

namespace ClimbGames
{
    public partial class UIPatchView : UIBase<UIPatchViewData>
    {

        protected override UniTask OnInitialize(UIPatchViewData data)
        {
            // implement ui

            return base.OnInitialize(data);
        }
    }
}