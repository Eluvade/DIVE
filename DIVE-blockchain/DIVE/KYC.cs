using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System.Numerics;

namespace Neo.SmartContract
{
    public class KYC : Framework.SmartContract
    {
        public static string[] GetKYCMethods() => new string[] {
            "AddAddress",
            "crowdsale_status",
            "GetBlockHeight",
            "GetGroupMaxContribution",
            "GetGroupNumber",
            "GetGroupUnlockBlock",
            "GroupParticipationIsUnlocked",
            "RevokeAddress",
            "SetGroupMaxContribution",
            "SetGroupUnlockBlock"
        };

        public static object HandleKYCOperation(string operation, params object[] args)
        {
            if (operation == "crowdsale_status")
            {
                if (!Helpers.RequireArgumentLength(args, 1))
                {
                    return false;
                }
                return AddressIsWhitelisted((byte[])args[0]);
            }
            else if (operation == "GetGroupNumber")
            {
                if (!Helpers.RequireArgumentLength(args, 1))
                {
                    return false;
                }
                return GetWhitelistGroupNumber((byte[])args[0]);
            }
            else if (operation == "GroupParticipationIsUnlocked")
            {
                if (!Helpers.RequireArgumentLength(args, 1))
                {
                    return false;
                }
                return GroupParticipationIsUnlocked((int)args[0]);
            }
            else if (operation == "GetBlockHeight")
            {
                return Blockchain.GetHeight();
            }

            switch (operation)
            {
                case "AddAddress":
                    if (!Helpers.RequireArgumentLength(args, 2))
                    {
                        return false;
                    }
                    return AddAddress((byte[])args[0], (int)args[1]);
                case "GetGroupMaxContribution":
                    if (!Helpers.RequireArgumentLength(args, 1))
                    {
                        return false;
                    }
                    return GetGroupMaxContribution((BigInteger)args[0]);
                case "GetGroupUnlockBlock":
                    if (!Helpers.RequireArgumentLength(args, 1))
                    {
                        return false;
                    }
                    return GetGroupUnlockBlock((BigInteger)args[0]);
                case "RevokeAddress":
                    if (!Helpers.RequireArgumentLength(args, 1))
                    {
                        return false;
                    }
                    return RevokeAddress((byte[])args[0]);
                case "SetGroupMaxContribution":
                    if (!Helpers.RequireArgumentLength(args, 2))
                    {
                        return false;
                    }
                    return SetGroupMaxContribution((BigInteger)args[0], (uint)args[1]);
                case "SetGroupUnlockBlock":
                    if (!Helpers.RequireArgumentLength(args, 2))
                    {
                        return false;
                    }
                    return SetGroupUnlockBlock((BigInteger)args[0], (uint)args[1]);

            }

            return false;
        }

        public static bool AddAddress(byte[] address, int groupNumber)
        {
            if (address.Length != 20 || groupNumber <= 0)
            {
                return false;
            }

            if (Helpers.VerifyIsAdminAccount())
            {
                StorageMap kycWhitelist = Storage.CurrentContext.CreateMap(StorageKeys.KYCWhitelistPrefix());
                kycWhitelist.Put(address, groupNumber);
                return true;
            }
            return false;
        }

        public static bool AddressIsWhitelisted(byte[] address)
        {
            if (address.Length != 20)
            {
                return false;
            }

            BigInteger whitelisted = GetWhitelistGroupNumber(address);
            return whitelisted > 0;
        }

        public static BigInteger GetGroupMaxContribution(BigInteger groupNumber)
        {
            StorageMap contributionLimits = Storage.CurrentContext.CreateMap(StorageKeys.GroupContributionAmountPrefix());
            BigInteger maxContribution = contributionLimits.Get(groupNumber.AsByteArray()).AsBigInteger();

            if (maxContribution > 0)
            {
                return maxContribution;
            }

            return DIVE.DIVE.maximumContributionAmount();
        }

        public static uint GetGroupUnlockBlock(BigInteger groupNumber)
        {
            if (groupNumber <= 0)
            {
                return 0;
            }

            StorageMap unlockBlock = Storage.CurrentContext.CreateMap(StorageKeys.GroupUnlockPrefix());
            return (uint)unlockBlock.Get(groupNumber.AsByteArray()).AsBigInteger();
        }

        public static BigInteger GetWhitelistGroupNumber(byte[] address)
        {
            if (address.Length != 20)
            {
                return 0;
            }

            StorageMap kycWhitelist = Storage.CurrentContext.CreateMap(StorageKeys.KYCWhitelistPrefix());
            return kycWhitelist.Get(address).AsBigInteger();
        }

        public static bool GroupParticipationIsUnlocked(int groupNumber)
        {
            if (groupNumber <= 0)
            {
                return false;
            }

            uint unlockBlockNumber = GetGroupUnlockBlock(groupNumber);
            return unlockBlockNumber > 0 && unlockBlockNumber <= Blockchain.GetHeight();
        }

        public static bool RevokeAddress(byte[] address)
        {
            if (address.Length != 20)
            {
                return false;
            }

            if (Helpers.VerifyIsAdminAccount())
            {
                StorageMap kycWhitelist = Storage.CurrentContext.CreateMap(StorageKeys.KYCWhitelistPrefix());
                kycWhitelist.Delete(address);
                return true;
            }
            return false;
        }

        public static bool SetGroupMaxContribution(BigInteger groupNumber, uint maxContribution)
        {
            if (groupNumber <= 0 || maxContribution <= 0)
            {
                return false;
            }

            if (Helpers.VerifyIsAdminAccount())
            {
                StorageMap contributionLimits = Storage.CurrentContext.CreateMap(StorageKeys.GroupContributionAmountPrefix());
                contributionLimits.Put(groupNumber.AsByteArray(), maxContribution);
                return true;
            }

            return false;
        }

        public static bool SetGroupUnlockBlock(BigInteger groupNumber, uint unlockBlockNumber)
        {
            if (groupNumber <= 0 || unlockBlockNumber <= 0)
            {
                return false;
            }

            if (Helpers.VerifyIsAdminAccount())
            {
                Runtime.Notify("SetGroupUnlockBlock() groupNumber / unlockBlockNumber", groupNumber, unlockBlockNumber);
                StorageMap unlockBlocks = Storage.CurrentContext.CreateMap(StorageKeys.GroupUnlockPrefix());
                unlockBlocks.Put(groupNumber.AsByteArray(), unlockBlockNumber);
                return true;
            }

            return false;
        }
    }
}