using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace DIVE
{
    public class DIVE : SmartContract
    {
        public static string name() => "DIVE";
        public static string symbol() => "DIVE";
        public static readonly byte[] Owner = "AK2nJJpJr6o664CWJKi1QRXjqeic2zRp8y".ToScriptHash();
        public static byte decimals() => 8;

        public const UInt64 totalAmount = 1000000000;

        public static string supportedStandards() => "{\"NEP-5\", \"NEP-10\"}";
        public static bool icoAllowsNEO() => true;
        public static bool icoAllowsGAS() => true;
        public static ulong maximumContributionAmount() => 200000;
        public static ulong icoNeoToTokenExchangeRate() => 2000;
        public static ulong icoGasToTokenExchangeRate() => 800;
        public static bool useTokenVestingPeriod() => false;
        public static object[] immediateProjectGrowthAllocation() => new object[] { 0, 0 };
        public static int DIVEFoundersAllocationPercentage() => 0;
        public static int presaleAllocationPercentage() => 10;



        public static byte[] ProjectKey() => new byte[] { 130, 55, 173, 223, 71, 209, 115, 2, 149, 10, 70, 190, 241, 73, 203, 204, 143, 58, 200, 201 };
        public static readonly byte[] initialAdminAccount = { 121, 56, 112, 82, 122, 50, 99, 105, 101, 113, 106, 88, 82, 81, 49, 105, 75, 74, 87, 67, 52, 54, 54, 111, 54, 114, 74, 112, 74, 74, 110, 50, 75, 65 };
        public static readonly byte[] NEO = { 155, 124, 255, 218, 166, 116, 190, 174, 15, 147, 14, 190, 96, 133, 175, 144, 147, 229, 254, 86, 179, 74, 92, 34, 12, 205, 207, 110, 252, 51, 111, 197 };
        public static readonly byte[] GAS = { 231, 45, 40, 105, 121, 238, 108, 177, 183, 230, 93, 253, 223, 178, 227, 132, 16, 11, 141, 20, 142, 119, 88, 222, 66, 228, 22, 139, 113, 121, 44, 96 };



        [Serializable]
        private class Model
        {
            public BigInteger id;
            public byte[] owner; // Model designer's address
            public string hash; // 3D Model SHA1 checksum
            public string properties; // As JSON
            
            public static byte[] ToByteArray(Model source)
            {
                return source.Serialize();
            }
            public static Model FromByteArray(byte[] data)
            {
                return (Model)data.Deserialize();
            }
        }

        public static byte[] concatKey(string x, BigInteger y)
        {
            return x.AsByteArray().Concat(y.AsByteArray());
        }

        public static byte[] concatKey(string x, string y)
        {
            return x.AsByteArray().Concat(y.AsByteArray());
        }

        private static string getModelHash(BigInteger id)
        {
            byte[] key = concatKey("models/", id);
            byte[] result = Storage.Get(Storage.CurrentContext, key);
            if (result.Length == 0)
            {
                return null;
            }

            return Model.FromByteArray(result).hash;
        }

        private static string getModelProperties(BigInteger id)
        {
            byte[] key = concatKey("models/", id);
            byte[] result = Storage.Get(Storage.CurrentContext, key);
            if (result.Length == 0)
            {
                return null;
            }

            return Model.FromByteArray(result).properties;
        }
        private static byte[] getModelOwner(BigInteger id)
        {
            byte[] key = concatKey("models/", id);
            byte[] result = Storage.Get(Storage.CurrentContext, key);
            if (result.Length == 0)
            {
                return null;
            }

            return Model.FromByteArray(result).owner;
        }

        private static BigInteger createModel(byte[] owner, string properties, string hash)
        {
            StorageContext ctx = Storage.CurrentContext;

            Model model = new Model();

            model.owner = owner;
            model.properties = properties;
            model.hash = hash;

            int seed = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            RandomBigInteger rnd = new RandomBigInteger(seed);
            BigInteger id = rnd.NextBigInteger(63);
            while(Storage.Get(ctx, concatKey("models/", id)).Length != 0)
            {
                id = rnd.NextBigInteger(63);
            }
            model.id = id;
            byte[] key = concatKey("models/", id);

            byte[] obj = Model.ToByteArray(model);

            Storage.Put(ctx, key, obj);
            return id;
        }

        private static bool updateModelInfo(BigInteger id, string newProperties, string newHash)
        {
            StorageContext ctx = Storage.CurrentContext;

            byte[] key = concatKey("models/", id);
            byte[] currentModel = Storage.Get(ctx, key);

            if (currentModel.Length == 0) return false;

            Model model = Model.FromByteArray(currentModel);

            model.properties = newProperties;
            model.hash = newHash;

            byte[] newModel = Model.ToByteArray(model);

            Storage.Put(ctx, key, newModel);

            return true;
        }


		public static bool initSmartContract()
		{
			if (Neo.SmartContract.Helpers.ContractInitialised())
			{
				// contract can only be initialised once
				Runtime.Log("InitSmartContract() contract already initialised");
				return false;
			}

			uint ContractInitTime = Neo.SmartContract.Helpers.GetBlockTimestamp();
			Storage.Put(Storage.CurrentContext, Neo.SmartContract.StorageKeys.ContractInitTime(), ContractInitTime);

			// assign pre-allocated tokens to the project
			object[] immediateAllocation = immediateProjectGrowthAllocation();

			BigInteger immediateProjectAllocationValue = ((totalAmount * (BigInteger)immediateAllocation[0]) / 100) * Neo.SmartContract.NEP5.factor;

			Neo.SmartContract.Helpers.SetBalanceOf(ProjectKey(), immediateProjectAllocationValue);
			BigInteger founderTokenAllocation = ((totalAmount * (BigInteger)DIVEFoundersAllocationPercentage()) / 100) * Neo.SmartContract.NEP5.factor;

			// token allocated to presale
			BigInteger presaleAllocationMaxValue = ((totalAmount * (BigInteger)presaleAllocationPercentage()) / 100) * Neo.SmartContract.NEP5.factor;

			// update the total supply to reflect the project allocated tokens
			BigInteger totalSupply = immediateProjectAllocationValue + founderTokenAllocation + presaleAllocationMaxValue;
            Neo.SmartContract.Helpers.SetTotalSupply(totalSupply);
			Runtime.Log("InitSmartContract() contract initialisation complete");
			return true;
		}

        public static Object Main(string op, params object[] args)
        {
            if(Runtime.Trigger == TriggerType.Verification)
            {
                if(Owner.Length == 20)
                {
                    return false;
                }
                if(Owner.Length == 33)
                {
                    byte[] signature = op.AsByteArray();
                    return VerifySignature(signature, Owner);
                }

                return false;
            }

            if(Runtime.Trigger == TriggerType.VerificationR)
            {
                return true;
            }

            if(Runtime.Trigger == TriggerType.Application)
            {
                foreach(string nmeth in Neo.SmartContract.NEP5.GetNEP5Methods())
                {
                    if(nmeth == op)
                    {
                        return Neo.SmartContract.NEP5.HandleNEP5Operation(op, args, ExecutionEngine.CallingScriptHash, ExecutionEngine.EntryScriptHash);
                    }
                }

                foreach (string kmeth in Neo.SmartContract.KYC.GetKYCMethods())
                {
                    if (kmeth == op)
                    {
                        return Neo.SmartContract.KYC.HandleKYCOperation(op, args);
                    }
                }

                foreach (string hmeth in Neo.SmartContract.Helpers.GetHelperMethods())
                {
                    if (hmeth == op)
                    {
                        return Neo.SmartContract.Helpers.HandleHelperOperation(op, args);
                    }
                }

                switch (op)
                {
                    case "initSmartContract":
                        {
                            return initSmartContract();
                        }
                    case "getModelHash":
                        {
                            if (!Neo.SmartContract.Helpers.RequireArgumentLength(args, 1))
                            {
                                return false;
                            }
                            BigInteger id = (BigInteger)args[0];
                            return getModelHash(id);
                        }
                    case "getModelProperties":
                        {
                            if (!Neo.SmartContract.Helpers.RequireArgumentLength(args, 1))
                            {
                                return false;
                            }
                            BigInteger id = (BigInteger)args[0];
                            return getModelProperties(id);
                        }
                    case "getModelOwner":
                        {
                            if (!Neo.SmartContract.Helpers.RequireArgumentLength(args, 1))
                            {
                                return false;
                            }
                            BigInteger id = (BigInteger)args[0];
                            return getModelOwner(id);
                        }
                    case "createModel":
                        {
                            if (!Neo.SmartContract.Helpers.RequireArgumentLength(args, 2))
                            {
                                return false;
                            }
                            return createModel((byte[])args[0], (string)args[1], (string)args[2]);
                        }
                    case "updateModelInfo":
                        {
                            if (!Neo.SmartContract.Helpers.RequireArgumentLength(args, 3))
                            {
                                return false;
                            }
                            return updateModelInfo((BigInteger)args[0], (string)args[1], (string)args[2]);
                        }

                }
            }

            return false;
        }
    }
}
