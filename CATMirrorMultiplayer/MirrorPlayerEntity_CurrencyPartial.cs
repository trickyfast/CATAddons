// Copyright (C) 2019 Tricky Fast Studios, LLC
using Mirror;
using TrickyFast.CAT.VirtualCurrencies;

namespace TrickyFast.Multiplayer
{
    public partial class MirrorMultiplayerEntity : IMultiplayerCurrency
    {
        public void Grant(string currencyName, int amount)
        {
            CmdGrantAmount(currencyName, amount);
        }

        public void Spend(string currencyName, int amount)
        {
            CmdSpendAmount(currencyName, amount);
        }

        public void Set(string currencyName, int amount)
        {
            CmdSetAmount(currencyName, amount);
        }

        public void ResetCurr()
        {
            CmdResetCurrency();
        }

        [Command]
        private void CmdGrantAmount(string currencyName, int amount)
        {
            GetComponent<VirtualCurrencyManager>().Grant(currencyName, amount);
        }

        [Command]
        private void CmdSpendAmount(string currencyName, int amount)
        {
            GetComponent<VirtualCurrencyManager>().Spend(currencyName, amount);
        }

        [Command]
        private void CmdResetCurrency()
        {
            GetComponent<VirtualCurrencyManager>().ResetCurr();
        }

        [Command]
        private void CmdSetAmount(string currencyName, int amount)
        {
            GetComponent<VirtualCurrencyManager>().Set(currencyName, amount);
        }
    }
}