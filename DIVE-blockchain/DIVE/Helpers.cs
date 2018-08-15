using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.System;
using Neo.SmartContract.Framework.Services.Neo;
using System.Numerics;

namespace Neo.SmartContract
{
    public class Helpers : Framework.SmartContract
    {
        public static string[] GetHelperMethods() => new string[] {
            "BalanceOfVestedAddress",
            "IsPresaleAllocationLocked",
            "supportedStandards"
        };

        public static object HandleHelperOperation(string operation, params object[] args)
        {
            switch (operation)
            {
                case "BalanceOfVestedAddress":
                    if (!Helpers.RequireArgumentLength(args, 1))
                    {
                        return false;
                    }
                    return Helpers.BalanceOfVestedAddress((byte[])args[0]);
                case "IsPresaleAllocationLocked":
                    return Storage.Get(Storage.CurrentContext, StorageKeys.PresaleAllocationLocked());
                case "supportedStandards":
                    return DIVE.DIVE.supportedStandards();
            }
            return false;
        }

        public static BigInteger BalanceOfSaleContribution(byte[] account)
        {
            if (account.Length != 20)
            {
                Runtime.Log("BalanceOfSaleContribution() invalid address supplied");
                return 0;
            }

            StorageMap balances = Storage.CurrentContext.CreateMap(StorageKeys.ContributionBalancePrefix());

            return balances.Get(account).AsBigInteger();
        }

        public static BigInteger BalanceOfVestedAddress(byte[] account)
        {
            if (account.Length != 20)
            {
                Runtime.Log("BalanceOfVestedAddress() invalid address supplied");
                return 0;
            }

            StorageMap balances = Storage.CurrentContext.CreateMap(StorageKeys.VestedBalancePrefix());

            return balances.Get(account).AsBigInteger();
        }


        public static uint GetBlockTimestamp()
        {
            Header header = Blockchain.GetHeader(Blockchain.GetHeight());
            return header.Timestamp;
        }


        public static uint GetContractInitTime()
        {
            return (uint)Storage.Get(Storage.CurrentContext, StorageKeys.ContractInitTime()).AsBigInteger();
        }

        public static object[] GetTransactionAndSaleData()
        {
            Transaction tx = (Transaction)ExecutionEngine.ScriptContainer;
            TransactionOutput[] inputs = tx.GetReferences();
            TransactionOutput reference = inputs[0];
            TransactionOutput[] outputs = tx.GetOutputs();
            byte[] sender = reference.ScriptHash;
            byte[] receiver = ExecutionEngine.ExecutingScriptHash;
            ulong receivedNEO = 0;
            ulong receivedGAS = 0;

            foreach (var input in inputs)
            {
                if (input.ScriptHash == receiver)
                {
                    throw new System.Exception();
                }
            }

            foreach (TransactionOutput output in outputs)
            {
                if (output.ScriptHash == receiver)
                {
                    ulong receivedValue = (ulong)output.Value;
                    Runtime.Notify("GetTransactionData() Received Deposit type", receiver, reference.AssetId);
                    if (reference.AssetId == NEP5.NEO)
                    {
                        receivedNEO += receivedValue;
                    }
                    else if (reference.AssetId == NEP5.GAS)
                    {
                        receivedGAS += receivedValue;
                    }
                }
            }

            BigInteger whiteListGroupNumber = KYC.GetWhitelistGroupNumber(sender);
            BigInteger crowdsaleAvailableAmount = NEP5.CrowdsaleAvailableAmount();
            BigInteger groupMaximumContribution = KYC.GetGroupMaxContribution(whiteListGroupNumber) * NEP5.factor;

            BigInteger totalTokensPurchased = 0;
            BigInteger neoRemainingAfterPurchase = 0;
            BigInteger gasRemainingAfterPurchase = 0;
            BigInteger runningCrowdsaleAmount = crowdsaleAvailableAmount;

            if (DIVE.DIVE.icoAllowsNEO() && receivedNEO > 0)
            {
                BigInteger neoTokenValue = receivedNEO * DIVE.DIVE.icoNeoToTokenExchangeRate();
                if (neoTokenValue > runningCrowdsaleAmount)
                {
                    // the user is trying to purchase more tokens than are available
                    // figure out how much LX can be purchased without exceeding the cap
                    neoRemainingAfterPurchase = (neoTokenValue - runningCrowdsaleAmount) / (DIVE.DIVE.icoNeoToTokenExchangeRate());
                    totalTokensPurchased = runningCrowdsaleAmount;
                }
                else
                {
                    // there is enough LX left for this purchase to complete
                    totalTokensPurchased = neoTokenValue;
                }
                // ensure amountAvailable now reflects number of tokens purchased with NEO
                runningCrowdsaleAmount -= totalTokensPurchased;
            }

            if (DIVE.DIVE.icoAllowsGAS() && receivedGAS > 0)
            {
                BigInteger gasTokenValue = receivedGAS * DIVE.DIVE.icoGasToTokenExchangeRate();
                if (gasTokenValue > runningCrowdsaleAmount)
                {
                    // the user is trying to purchase more tokens than are available
                    // figure out how much LX can be purchased without exceeding the cap
                    gasRemainingAfterPurchase = (gasTokenValue - runningCrowdsaleAmount) / (DIVE.DIVE.icoGasToTokenExchangeRate());
                    totalTokensPurchased = totalTokensPurchased + runningCrowdsaleAmount;
                }
                else
                {
                    totalTokensPurchased = totalTokensPurchased + gasTokenValue;
                }
            }

            BigInteger totalContributionBalance = BalanceOfSaleContribution(sender) + totalTokensPurchased;

            return new object[] {
                tx,                             // neo transaction object
                sender,                         // who initiated the transfer
                receiver,                       // who the assets were sent to
                receivedNEO,                    // how many neo were transferred
                receivedGAS,                    // how many gas were transferred
                whiteListGroupNumber,           // what whitelist group is the sender in
                crowdsaleAvailableAmount,       // how many tokens are left to be purchased
                groupMaximumContribution,       // how many tokens can members of this whitelist group purchase
                totalTokensPurchased,           // the total number of tokens purchased in this transaction
                neoRemainingAfterPurchase,      // how much neo is left after purchase of tokens
                gasRemainingAfterPurchase,      // how much gas is left after purchase of tokens
                totalContributionBalance        // the total amount of tokens sender has purchased during public sale
            };
        }

        public static bool IsContractWhitelistedDEX(byte[] address)
        {
            if (IsDEXWhitelistingEnabled())
            {
                StorageMap dexList = Storage.CurrentContext.CreateMap(StorageKeys.WhiteListedDEXList());

                bool isWhiteListed = dexList.Get(address).AsString() == "1";
                Runtime.Notify("IsContractWhitelistedDex() address, isWhiteListed", address, isWhiteListed);
                return isWhiteListed;
            }

            return true;
        }

        public static bool IsDEXWhitelistingEnabled()
        {
            bool isEnabled = Storage.Get(Storage.CurrentContext, StorageKeys.WhiteListDEXSettingChecked()).AsString() == "1";
            Runtime.Notify("IsDEXWhitelistingEnabled isEnabled", isEnabled);
            return isEnabled;
        }

        public static bool RequireArgumentLength(object[] args, int numArgs)
        {
            return args.Length == numArgs;
        }

        public static void SetAllowanceAmount(byte[] transferApprovalKey, BigInteger amount)
        {
            if (transferApprovalKey.Length != 40)
            {
                Runtime.Log("SetAllowanceAmount() invalid transferApprovalKey.Length");
                return;
            }

            StorageMap allowances = Storage.CurrentContext.CreateMap(StorageKeys.TransferAllowancePrefix());

            if (amount == 0)
            {
                // originator is revoking approval for transfer - delete the authorisation
                allowances.Delete(transferApprovalKey);
            }
            else
            {
                // authorisations are not incrememntal
                allowances.Put(transferApprovalKey, amount);
            }
        }

        public static void SetBalanceOfSaleContribution(byte[] address, BigInteger newBalance)
        {
            if (address.Length != 20)
            {
                Runtime.Log("SetBalanceOfSaleContribution() address.length != 20");
                return;
            }

            StorageMap balances = Storage.CurrentContext.CreateMap(StorageKeys.ContributionBalancePrefix());

            if (newBalance <= 0)
            {
                balances.Delete(address);
            }
            else
            {
                Runtime.Notify("SetBalanceOfSaleContribution() setting balance", newBalance);
                balances.Put(address, newBalance);
            }
        }

        public static void SetBalanceOf(byte[] address, BigInteger newBalance)
        {
            if (address.Length != 20)
            {
                Runtime.Log("SetBalanceOf() address.length != 20");
                return;
            }

            StorageMap balances = Storage.CurrentContext.CreateMap(StorageKeys.BalancePrefix());

            if (newBalance <= 0)
            {
                balances.Delete(address);
            }
            else
            {
                Runtime.Notify("SetBalanceOf() setting balance", newBalance);
                balances.Put(address, newBalance);
            }
        }

        public static void SetBalanceOfVestedAmount(byte[] address, BigInteger newBalance)
        {
            if (address.Length != 20)
            {
                Runtime.Log("SetBalanceOfVestedAmount() address.length != 20");
                return;
            }

            StorageMap balances = Storage.CurrentContext.CreateMap(StorageKeys.VestedBalancePrefix());

            if (newBalance <= 0)
            {
                balances.Delete(address);
            }
            else
            {
                Runtime.Notify("SetBalanceOfVestedAmount() setting balance", newBalance);
                balances.Put(address, newBalance);
            }
        }

        public static void SetTotalSupply(BigInteger newlyMintedTokens)
        {
            BigInteger currentTotalSupply = NEP5.TotalSupply();
            Runtime.Notify("SetTotalSupply() setting new totalSupply", newlyMintedTokens + currentTotalSupply);

            Storage.Put(Storage.CurrentContext, StorageKeys.TokenTotalSupply(), currentTotalSupply + newlyMintedTokens);
        }

        public static bool VerifyIsAdminAccount()
        {
            if (ContractInitialised())
            {
                return VerifyWitness(Storage.Get(Storage.CurrentContext, StorageKeys.ContractAdmin()));
            }
            else
            {
                return VerifyWitness(DIVE.DIVE.initialAdminAccount);
            }

        }

        public static bool VerifyWitness(byte[] verifiedAddress)
        {
            return Runtime.CheckWitness(verifiedAddress);
        }



        public static bool ContractInitialised()
        {
            return Storage.Get(Storage.CurrentContext, StorageKeys.ContractInitTime()).AsBigInteger() > 0;
        }
    }

}