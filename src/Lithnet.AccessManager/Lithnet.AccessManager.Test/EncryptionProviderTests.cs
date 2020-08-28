﻿using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Lithnet.AccessManager.Test
{
    public class EncryptionProviderTests
    {
        private const string dataBlock = "{62ff735602e84c1f92445ab344fb68c625a06306dd9b4a71bf33ad7b02c58006434e8cdef75f40d39ea87889eb026739478b96dcd5eb4e3c876aef068b024e4775d71936744d439dbbeead4e9d295ca64aa34d16a25a418aad74f0fc3e11fcf1d6b596745dcc4f10aa7b08cbbf2311dac67c21df84504cab87c37861e7c1d84d600754764920420681239a28db9710d52f1265652e814f2d8925baf15a02fd30adf297894709436d8afcfe8adf715d9bd11e6254041a404598868b8fc3bdbe6e6287089e16a14e28b8d23bc64b9bf039499ae485e8ce4f8a81c2f44894c70a2c19e0bc6d1316446bb2db2ac9b9a999df49c5ccb9ba3f4f3f9e52ecb2ad507e8963cc687de5dd48e3a801572e65bb1cd898a73648a0254215b8af499e01b9c116e61eaec6480c40fca7fbc08eadeb80df161f640db0d0400aaa3ef768a6cd280b5efd77492e484f769f39f0b2144b81259fbef425528d42acb5d2be0da7c61603413337dab55b4cbda1c83607b6f8e89d63662cff52bd4889a2ccfe544c2be9a081644be3f11547569e404f95744a8252b2cb723db8534aadaf1cd339616dbe9f460c98e02d954f7bb97046900d2cea1e0fb602df58574b8ca19b8b31d8ae50dfaf250367995d4f38b79d1f2c17a00c4a875446592bd240b6ab2fc85e97ecd4f5756b1f9632f843d9b3e153adf3ce36888a44c884d78e43e5a71038c968ec6a145dd7dcb1319e486196c38f05c38fecdfe37caa85f1fa4d27b463561ae73b570d42c50eb69963499d9340ed35d12062a90e77df2030874035ba628416e497142d348515b11e064470a1242e9765b88df24f35c843c0cd4f0fa469de1979f81a0224074ae150754d02b1f9df4aa00df4cfa79bd97c9f6846b4aae5630f78c62955a2a4a6cc7f734b9d8276d3bca107e9c88e39df85a3764d1184e82b6f70f3a6642932c8a747064cf8a924bbd01bd49ca20aadfe929afd4894bf841ed4564431cf4ba362b4d134436ab9808e77af96de89b47c0aa600474566ae799a95d48968085b210da74f18474c887a31aafee559cdd58d517704d84dcaa0ed6c059aa9008094253b713088435aae924cf50202d144017784c913ec42f78824a1d538b81a826b8176d0d8624c58bd6430a9be047f180f8a291226bf4bfbba94275883de6111f1d64114383f4df3a611ebd2ea2503e01e0c0e050aaf499da02bf668b84499362204a7c5994b4e8eadf78d8bfc4c9fb3d175c16300c44dd69029d5628bad3e6fec9d9362c0fb41228f065b9aefda9d052017ed65d88e46baadc672596f9cd1cba206a502cd484feeab2eb8081de8ea97d1d62600ec6b4718a9ca6906cf41b2d3206953cf723a4e23979048244fa10ab22389e6f22a2e409ea22b502c39ad17c08c86693b69d740e58972882b7c7ec6ea4d3a559180bb4234a5c687e11a977eda280a63b37a69404f8926b130bbd880c538c97e641f7b4d1db683f4e4a17983e505931eb72dcd402abaa93ace03796b6748eb575e1d0d486aaeeb195bdc65bb8453a501e8e9094d1ab5833c6e9c85bce2e10b20237e9b4114a03b0af14749680ee37fffaaaeb04e038dd9df7397e8d803db3203aa64954300bfdac9828e4b7b4ad40ccc6fbed6426e8d5c87e1a321c82724044e3cdf424740ac77e159fb8ea0b4647d50e6ee624ef983429c2586e1a348d59a0767e24b41839d33649d61586adddfc734057f69467a93608656b013b97c5e0fd6c685934a6a8af3c8902ac1908cd2d08e8692174debae4d649f7c1576daf880e5aeddc345d5951e1da420ac00958312dcd2b3e44800a6760fe04ad2563143c5236aa8c74494abf005e258ddab2e711140bbbe1e4c0fa873a9e7f07c8683cec8c56a69d74e2f8fdddb8eb87bf9c5acf52aac95db4b0db84e74eed422ec6686d1ad0dfa7a40d2b8d17d2895edb13e5aa0545df3924eadb3e0dbd5e3537c9d45dd4be6d3d246db8392fa7f050cf18f367445e3ed874c9abb5ba8e7fed63f90bbce8259f8d3455198bfeb3ca082240e78212b9ac7ca4913852a24e276e905b26d25f83f54ea4778a2b7a62cd5e0750ca846a103ca534224bf9f8f16c92cced5f614d043aa4d4be6ae8b3414df9bd87ec31cd5b30bd0461b8f5c75c2da3ac8a0470e0207639b48a48fd96a2d19ef50fbc058e2fcf4124602bfca3c3d0bbe8807cad835c7f7924a88bf993d5de5459517054d2d070e3b47269330a10bf488bcad89a01c3cb966475a82b04fe0d3874df094cb677f88464be985b4b8ef61c95a1abbb54263f7b0492db0cb667fa5b635f12e379378c13e45ada5cebcaf873fa5d92d2c9688fea54984a3aa9acba263d00bcb288f6715eb426c822ff682f35c213fb9196ec7f18f4961bb21ba907db88bab725bbfb973a34d63b12726b7c91731b4b0d8437ad8464f24b7a5b8e1500337757e46e6da00b04661bfc66f5694b718f07d01797d5c2a494a8aa43726d2ee57ebc3c6667dcecd4abe92d2f57d61560f34b99c7a637d0148b4a5b8f2cf877eaf2c68a56bb2c4bf4faebebfa55d0555db55e3d622784ae24528ba57d949d2a9b78cc7097c794f374903bcc3fa1b41d31fe8ceb5d9ab2bfe4dba93b1ca62b3cf4c67c0eff6f90c1e4b61835e2b9fba2e84130d8b9b999bab408b9ac738b98be45dc12a1ad53064b64a5fb37f2b10606a5075caeb14545cf54d378fe21fa3c3eb6c9775ba3488bbb446d8a4914b73108a780db78e7c25e16d4f999b8d115331acbe46514cc1a8bf274159934558f4d01f6593a2c3080427af4f559a11fd498b56462b1ad082e8edb849ad8ded2fdf2a2c7c0795256c5eccff4ae49197633766f2d5da084b440d988d49188e3f45bd34ef9d234d229c2c47ac4255acb5f156393196865a54ba8738b049458567f1e749628023bd4d640ef44440009ab6df1b51d2510ba1b69be482e3409f8ff200baf5404a033acb569301b546c2b8067946154264ad9c8a01875709454984c7d4fcd7bf2d364fdff0174d614539b8546c6718c6e8fd78e5825bacf04133838e02898aa184463487aa33361442e9bffae4b3ddf3704b290b22fec67544e3966fd0e549e16050d69282f570ae4723afbbeb4b39707c6bf37bbfad33a04fd18519f37d189b4f99c899d3f7db8840ee83d923fba0b531375e60633241de4abfbdb09fdddf29be7b0641dd5c9e4b4a7bad52efc5073be22c7aaddaf9d26b4537a00579200573c4f05647313386b348c48b4f938e66805924f6afe95652d94364af90533e8db6f238cb293a9cb96e4ee0b99a2abefacc250fbdbaca943d434ce49bacb583d72bcd320b477e70b6d34882be7c27a4189cc2e9a942ba0c98d7421a83858038c92bc6200aa147ce90d44204b06ab2166700c8ca3a6b0c0afc4b42fd9bad3ced3d769357142681441e1448e8b35ad86a10146b1f59825be25f74442aa51848c4a1e33bcb75fab212db4c4dc2a0e5e7d14185633b305a0d985cc24d5cb87869b7f63114188962f6c370a9485ab4bed386311547fdccebc028d2e24d298aad0feccf970260e2565a3eaa0344449c44141a496a4f9199c8ec25600c4e57b24d0dc5e459f4f589da494e95074385a23a0fb30a41a9adb425ffb4b9564ff1b0eb4dc509b88510fa87801da8494d29ae060a9f576a83378e9aa5b660914098b13ef69c9cef37d8ffec222eb5874bd092655acc43b030b4800bdff54b2e492989ed79f6a0fd78099bb9ef4c561c44498eb5b6c1324a00e50b9cee47212d4f9d90e4511ee4adb675442bb9d1f2c143b191e851354324014739bb8687278b4f82965a301998016613fbac68c74f1547699ede4ff10213bffce0158ce1ad3b4e74b9eb3f50a96662f98304338b60dd48c5bdc7e0baf8badac2328db9e91e834a69b18a23257617b5037d8e3acaac2d4a25ad0cc1222d7225cc60f36959de404e75ad3db007093d43cd426a346156ba405fadfa6ce18de4d48b21889b7637554c7a826717e942e17f308f9792dea2504d61a9de1d3e65751226612e688deae34b3eaf9dc3628568f4202d3f07f5a34f4fb0aa8f6d8161fb05ad153b0859bd47485191557104b9b338450784f6327e774f43838fadf85df82ff32b6b9bc3d72542328d791ef1d8dc28f05b461bc2b26943b6817f71524242ba87c8ac44372317497194f8397eb900ed3944420245ad64474c89739f7189cfe6313793a07f974c4ea5888ca5735a81b655e1ed4fe49517435db7419f1e384c9ea48dce7e7a699a4f68ab7e39398018ddb6ae65fe2389be424387c04d958d288c4aee65a325955045e1ae563713f2374c26b787e17fbfba4f89a01628b0220430b0d22ee3b1082149bd9f622ba6cd28cb06154b0818d01c4058b104b07a376939900dcf20f5d665474e985382d3617818e2971b3ebdcbf547adbc87ccb29b89a7944ef7c494aedf4807baf5a9ff4641b8c78282a6f5fc2348b2a6842bf0cdbea57fff33acc11bdc44c6a295883ca067836cd923f227965c4962bbc4eb4c3b54ca463e051a7f73ca4796bdcd27a4bad6cd873ebe4cd9c705411da2d249706553744a4e1d2df8847848eaa52b263afdff1581355fb9d2c29348d3883a2080e803c21c8590c89122fe4163b1b0934b21b4d3f4ea70867d44874d589006cafa95c793c3f53269c2240a467aa1bb71522f4dacd2905206b4624946ca939d5567f75d09ae9f52147290f04980972948c54893b1d6a4b3723946524c1695ca4554f6c2b806da3c33077de8476daaa2a312b7925165315368993caf4b4094f350de55de525260e77e9c04bb464d96257259531895289210fd27483440e7bb3c46a38f919ce263bd9993c8504a59807dd0bb6263a3ba6cfcf019c08e4b33a09e921d9cf9c6e9ee1df7fdd557445dbbbcb3191558114933560c0b9f9d44c2b263655870d0d9dd0c485ebdd6e74ab1affef486a5675e41eef3af0811e0407099d9359ddd107a15b3f80b564d9343b5b3f94d75635d34ef38b0b4d7ff914d0f904b24157379b07f6bf7e09c79a446bc8fe12181381c378d194f41eb49a44e08a5b480661049b3eee6e3e1ffa5c847478bdbd109f9fb947a7b3169feae7b4fdb9ad8ff9da1e27a588d3d7720ccda468695776fb7f4365806cd3d184752f94fb5b17df7afabf8ccd1fc5952c2dcb2409195533e678dd61051b15114f23c5945b69d75656a8872b90dff63074c260846b9be6487894eae829dac5d9eb8f7a74c708947e688f096ed07406679a750554429872261ffabe68aeebba0f237186e4a4aa43ddb19292cffd85d9fac663e474c4f9e37c1850d7723aa283666b537a3446fb22ae594cde4e4e02bcfbce10a3041feafa457087c5bc52fe10efb8cdf13434e9aa70a2f2dd4150435f01c52afd647b399e0999e8c5e71a4105b2f3d7a234980be277359df215b562795b02c9958436a96ea2f85ec7a4f9d4d3e5bb6e6f746e3a6f9d80734bb51b1b655dd25d9ef4a89ad479268628ca5626d73d847b9044792bd6d97786f5430034b3c16a4557642e89411f4e89c8b9e81a1528ed3754b46cb8c59459f5c5201d2fa102419f0ae488789665346c8db8b9efb9490b97c8f4db69fa2820e8ac572dc5ca1ac55581d4adfb6148e36a7049193cd96d658910d44bd99263cc9a7a6d2e7ca384f9f9e944b36b3b3063fced930ce3580412c2d0a498487d497110344c42b11806718c74b411f970d8c355f5bcf67aeb28750f39b4476a8375956d30fe2b91b2f9bb1d288431081718c5095e15ceac8e0670e410746b0ae85578230831ae753e6d53ab8804465be57ec15446758bbfcc7017da74b4c358de5fddacfc57f5a0646d266f3a542d18b8625d77bdbd31fdcbcfa0343fd4496b64954b9aab0c099ea5a20a8918446dcb059da0c6ae7244b1fbee1d9c2314f5e90285e26d2c97a592f70ea78d5224c46b441c5c49716a0af77521b3f9f2d472a88b10ca833ffca93507f953aa98a4c2ea7fb9de4619979e6e444445467fe4bafb49a6689162421a91c7291a7dba54b6681f511a2efd9e6d08a5b24374c5a47a9b0d995717255cb273c2082fbb9be4d5083ecc5572eb64b5e57e3b4201989422aa82ac28fe1573eecfc6d18d898724110ba695062bdf126c8ce901128335b4895ac0b88d6073dd664213c90713a794db6b8cf1169666dc77053edb1a3ac7a439fabe0e3d51eca6ed0e971469d485443d39912cd070583033fa06e8025e17b4235984a58c7ed0bc8db2faef509ef0349928ba8badd60be2b7bb927470e2e62411f959202dcf4249890e55b08091e5743e69f435843be20f7a95ecc0513f3cc4e72b68e569f8c8ac9cb0fdad4ce758042a3971c21d85eb325c258d9e16593d34a1bbacf11bd37761adc1c2d5381d9384d179d7ba1c7902d29c6a07de9486f054e46b403d03f6ffdb7f7047f4e4b4c3843ff84e8106432d0bb88c711163330da4b4aa67f4a71ebe70478b4ac337710044fb1ad9b4950d45393aea8f0a6820e8642b9ae701633bb37b76b9863ea22260b43899a28bc2c0b3bd099ffd55f9e856a4edeb3f0f6338a9108f6dc1a6ebc926543038b05fca57e97577d3108a17560134279b3b6def42b60d115e6a7ea62e7d64e78bb9ff322f2953cbc37b766dfa2e947c79cf1c5d71b2815da0d37f55f767a466baf67daf1c1be481519d8e396b48d4e39aba9f3524054a59d140f181e81c841758bf6d812649a1706520e9c9033ff412cac724ccae2bf813d972bc814601349e9a09d738eb624da76b64c1c0bf1aa412493c3e5d73822bde4485bbcb99627457a9fc8936ea201cd4f74882f67a55c46e7b75349fe442c9981fa9f150ec8dd4893ae2eeab84cb12d9cf83ad9188a8148eb917294370d948467406266192c3942c9a385089a13f239b3cfd2fb394b8e433eb4556c56d477c6c2fbe18ba633f549bf9585275e4ee3b47bcaf8ccd12e1340c78a6062ea528555dd92d962b2e8994de1ae32835721000832d9b93873665e4c48bfe79f5fc0e9ad7a02844613b4264e119a7a556ce14522687afb14daa76a4b729f54b02d8cfdfabca5fc0ba4d1a84d33aa4dc4dba02190074f5d0dc53d6143cc84b2af00e6fd1f7806b9851e917d47fc933ac82c27bd21e78b9151bd5c1040438eeb52c7309be724366238d3ef09464192f94cfb06ebaafc8e39e9bd33ec4bbc89e890a62ab2da73adf73c89a7ca48feadf7091c183cbfc84163212f2e7b4649b5f243198d4d83a40e374ff9a6804007a8e0c8a2732aeacb8278c4d7ac044a04ad2b228ab6fd4dd91550a8efc5c54bd5a2fdba4ee1846b586283bb37ae0d48fe99493f2d38bb42b5c9445c9d80ec46bc96d1e286e160116e10be191b19f14fafaaed3541a5893425c91aa8b9b30342479065eded5f465fd1c9926a7abd764ec2946b72109aeb9df55e0f25d7abf4436f83806490c23f3eb82e12f033bf614fdca1c7563380cec4d84f1be95c344d40ba818146d8634787ac4f8172deaba34fafa28a5794d3f6ad327fcfab9df3c44d1aa2eb47bca2ff9279d034b7a465ca421f8b533d185b2c6f4f58e43e80e33f49aa88b64369f7408e78a43951d41d324a909769009f7d7b416e0e6f2996dfd3476eba8e223a4848b6ffd22f2bb6eb234c48a94c4fd3ad8864fbbc92c65c5d564b7490ef108594900ecad5d1422cec0746cb89d9322e4a4a4468037e062545b94562821c4f4f6742d82d4bd1fc9a85f94e70bacdd98a26f5009adeec8b8889424c5c9740aaaf1419097a4e56a7dde70949d8a9f4f70d9fa597515334c1f6e5854560925496b3dcf48e40c549e89ce59b4753af53cb2c0552798f1e0dd34419e24ae5ac49b3eb16dc36213a4ca9dd1d144106af92f98dc6033ad1fe2c0bcc923e4eeea80922befce6ae6baba5a719d898443e953aabc3505cb6f0fa75658c64304dbca3d21362fab6cb2c80fc8d8b4b984f8eb129ddfcdd76af152da18bd6e4fc4dc0b151b2126673b636deae16e3bc2245be8101d2d8e8506fc308751fffc373430abc4c400307b91aee1a78d8e70d5146c68bf668ba22cf0c470ca545727b274cda99a8b46e599b2a7486d18ff04d76455180dbda4f5bf2249716f8023216ef4bceba7327adc07aa9414cb365137474479792440f495ad5baad4843edebbcba4a7bb5cac7b8b096f4a554012cfae07f488bbfcc67d7fc9a15ef863c97b5f5cb41faabee99a689f0ec9754f5533208004a889204d5275fac8c32dc9ab516657a442e8a5c46762639422f815bc119ee144d43a1581b9ec6f4a24f5e61dbfaa9cd4f88808bd1e843bc50c3936e416401534868919e3d0e2cf2e40dd543f23727014e5f824268f2b46c3b9b4b337da7220e4dd4930cac372924fa7cf02b531c9ecb4a46a4828a19fcd681c28b0e6576ff1c4801a39352bea31fbd4a4a4582e444b94ec4a644252480b3a50c2ac34de98fe3440cae679006a0e038b78c21dc7026aa482a8902ed2a231e640c4d587cd308cf4d86af3ae0364fbbb17e022fb60850014918a43e6717d930235447606bd19ded46ffbf86fa592d63b8cdcc0000a8b32546d292e10334907944f8f42ca3fe9dd54aa98845a3bcc22c48d718a060fd69314857bff787358e4126416418fe6f6756476ba41e377e96c23785759e3a0e5af74c92b0e8d4edc0bff0457256f8de0fa34e89bb988001c1b99b495142ea188399433a8b49cd341bc622d23e781d242a4141bcb97dda9ccb7e3bda3dc27386d1324ae2ae3c176a4a9c2fa75ab62418974845f5ba834a9be316428c21709110c37b4a118079f8ab49e747171c94487da1d14764a6f868ef4eef39ff3589f6209c7e48078ce43e1584379589645e2a99234949c4ad59d9ced28efefa91ca74cd718f46faa319a8d352ebd4d0b44893f9d497425ba18fbddbb21840ecba8fe53a35664e8eae91eea8cac58ae4e627b614b72d449fab065a8fdf0d3883b268d74de2184258ae02d278c0bc9c4b3abc4a65cac840ca9664dec5fd1a0e2c39fa05aba035473bbce639ffdd22ad3042ac9c0f53ce41cd82d927c434db98cfd9fb056145994281b3ba31cebbe659185692ea491a1f4ecbb5356576d28672a8d9896d5ea41549a5b401cb36c642b4059b97ef4585c74ae286b3dd42289622141661ee61dc7942cebe605e637d133cc219f8c9f3c8e543f5955a19ca5b1d7920cb3b6658ed1e4d4c8dfbad1b9cbdb63f7093225cd4384edcbc8ae7009d92d9bc3678736c1e68432fa9764242a1cee3b0268ea275ebf54e4f8c97fa8e018d482e2eef8673b04c4aa0a9a0585da7cb09d49247428e26cb4ac2a2740a045e43a76df6f03689a7274c5eb7e25ff7ceb1d8643afc71794453475994b53afa183bbc2d2541ed90af7e41e7aa7b57e45dd67ff7b8923da6d5ff411fb766f573e16c9e43c3bfd04c0db8477194392fa3aad2c4f953dc58c6c21a492abb012d01e57794f12c521b8db42f43f5b6d2879c96d919811ef4df26f1754c3a96e73e01f60311047775370546e44f27b14e0693dab27f0d7274434e6f354dc18418e9ab05dd40c709b7f04650764633ac03661178c8d5ed793e2126bb0140ca93e77ee2cf5e68b65a6f2cdd051a4b8f908d0199bc486d6b5086f3a6024d4d118d5c2cdc80d1a4576f5bdcdc63fb45808b5e6cc0e7be4c1b26f95aadef614c74bf5c53471e27d80dc426806ec9ae478d9fd22d70feae11c927aff88e5863421399b9c984e93d6d4111190c51bf604d4cb27a845fe86713217738c32abb5942b7862bfa10b4be14cbf421c811efd045b4a35a8025e65c7aa22d57249d36a54e4b8f99e3d07ce313a2d65c401422484ad0bf23ebd54adc1da1e53d7b1a7dbb4c5ba6f3952f3c628ea20ccf939d4cd54ca4b5777b107e5fe30ad2c86a72c02b438eb69ee268c244ac697590b1ab1a574a8ba44d8b07ccfca8820ba6a7503ae74c52bed0198e548be25ad1026166cd05462cbba1ce87cc224648be903c162f384e83994232ec04a10a72e0ff089c3998479fa227851c8e16a4bf09eba70d55424db6b71ede31c6552fd8cc40267e17b743b89108b732199a1f9564a50bdb74fd49ff9e9ac0035b17f87dcbc4a2e0cccd453da5c7b98e038692959c3a67c0aa304faf8a8ef79fd1c212031ac795fa253c4d829afb9013d2c259763ba3109941374400aaee850fd88d3a4c0c556cdb9e8842069a12298abb105b262b1a4eb505764985aab4e88e20ef7932ef09249521de45dc9b9c8f825562b628df1f42fbf6d14a5db6cbf9dfcc3417a5700e180385e447aaaf8f87216395f5a98e63b45a7e6a4a55bbf401a6ade90f950b3bef6e1449472796115794407d448dfaf5803250b34246a3ad6d010f1252be28dc180babf74661b4b1a2893804596e9ae42008453748618e5bbc6815702214b7193c1b858b46e8abc0cd3b8f0f4cdd67033d30520b424b82af1d9a0b2a4ac26ec123748a59462f8d4b88951ceb994e91299c91d3234109a94b7a9c72ad50a8ee316f94938b48248f4f4f16edacbab5681c94ed8d78402e9761a4e89cc06d29713af19bef5a4252ac951fef1f03f31fe161d96839c64b9eaae3e115f4d115ea0b24f474ad9e47aaad44e6d03370b32510f5cb9310f24df08cf7c5f9bcee54bd8b61fa1b9f384973bc04df513dd912d6eea148939a9b4951b7eb76390c67a521dca6dbb5dcb2436bbd3c22717d80f890734eaf20f0314246981782729be36ce515a4e7013bfa4314ae60f4dfbfd29f49dbb1196674dd4aa288ec855511984e94fd203fdccff7469a950a5344cda54362eb605db222364a35922260b8d76a015d7b7ba9df8e9b436c96c7b67b4c852bb11834ebd20c24475faf90459ec2b83327f6f2581b69584124ad54da7ae94cd1eb22212d4092bd4c1d9fabc5c7927ec4c20e711e3d68fe4d7ebd623b308c965201b4cde0af63024ac7b99cb25bac41b94e5115c9d35dca4d828ac8b7da620a410281ac8b7922094cbe9d5dbcff8753a56986c57a0d5d874ec3aa63f4e7198becdd77faaf9ec88d4beaae0337538994f4afeaa6ce6dcf714b7bb29b6263038d07644da408c229bb40fc9dc00e92279d28819a7a80530ac84b6ebd1af5163aeb8758d31eee1805e347dfb82533fdf238fab355d3a7e6c9c245eb8818e324842b51e55c1f9b2010b84dae86d2763ae8f914b5c05a4083be674923b3bd5fd1607feee9af1f12dca68243dbbd6d6573d3422c169b285c4644e14720ae8e12feb08d1d95b84d591a970346a0a521d71599efaad26f410ec3ec9f4235b432fb90d99b8ede07e74fa5eca3421d87e735a13393fc3186d8bd8c1d0d48f086c84517db6e8c066c1d4f6a7af24f2f80dc6951c1d89a24511e043c747a47398e79f1e1dd98f0f19f98c4593d2d4966a7cb60b9ad905e75ee17d26e62ba4ca197df73bbf5d2144f97ddf50cf9e94c49a07ac84f501f2a7e7b451dc01cc64fd5b7c40508196fa2e418f1f475862f43e4a05287e2016dbf3927c2e04fe32343d8b0f09b2e5a5029d20e3f9ea224f043aca22ed51d727b2ab6426a015286d24aa2bfeb79e6cc648b1e4ff807b1353c4077b6e4e4f0797650fa12e712bc2b684833a35dbe932b1508fe83324648e8b24bedba56b380600b5162c7437f957b804ac79ca9f84d3f80c84e4d9088c18a5749f693e141c1c3c771f95458809296de40ba953aa3b3ed46c2bc8e137f5388194c1e9cdc12edf24666fa1d9818fa552a456f8f925e4c833ee20217608553bf4e4ad6af263f438488fbf3d6ad3f0810864a6ea11bb7acc2c57252008785610bd6473bbd4dae62c6dc638b7364777445fe42e19e399b147b96d2f297496aac4cb54710bee09dfc79fc4f05c6f3809ad7504a8f9a8c6b91c0f454dd45d0293f94254e398c8cd2294b7277022d72c8943c1043ab8c629f332592f72d98bcbb0ea4454dcc8602fb5386a485820c19bffdf38d409f8d53a3c1d0c5e42b802aa8324f1d42e7b90393e8a3b9dde30fa7aa2b049e4700986d6c661904eb62022d766704ec4d61a91e7996500a6ae1c6e1a0ab8d4e432391ecf38668e412ea830c1509923448e3845d2e2d979d913cd0920cbafc2049be935fc3ac6d99287993888d9874244a9890a61a40c325f54e0cfda8c2878d4afc9f74fe40975e315beadb03735fc84e42b99fc4ef9a0047005f3cd0bbacfa492db04d8c58e407e0f719a47b8cd6924059b26d6ecceb4e93c1c9dfa22b62e245dca70c12be8888dcc5bca3fc0820b848ed9e154df35bc35b7835217bfdfdb346b6a6a2ecf6a2d1c3c77fbcef277afe47d29713459cd4de94a2198c8061aa0e4f40af8557ee6029991ae7cbd75d996248cf8d055f96ea2a3ea453e13cf150bd45238879cdb5198e4fe91ad58bbdf98e4743ba1efc52dfb7065416e07fc6998c4ccabe392ff1c41eb274e6d17b43424e42239f58ca3b1523ce1d7f97f5a179764d91b2861b058fe2bafe3c5f5b06486142d6aeed4cf925ad174a30c87a8b44b84f7daac9a578f269ebf757859d099b294f2aad47a239d585e7125abe18de31c2462db5315c137d1f8cbd6ba8927bffe0449d84cf7335aca3091a81e83c1ad3fc4ef884f4749d07aa9af71e2e01a561234662a707d7ff9ce668d1f966e5d8ab5646f7bd5b326ac033717dda52e8c8c8a8474ca2be11b913ef34589665d82f635448e3b08879b3dc0ca5cb0afcd820902d4b6c9a703cab987dbac8e2037a34b31241148756c106b6acdb584cc29f8923ec4660bd3de7a227c8b6220127c6850c884f12aced527d56f6c926a2c7063663c64b2ca4547cb6826218195a8bb9113ab241109ef9cd4efc9a7b5478d9d7657c99455aa69d4b1783b70512b22fa323cf164966b7c3e3357fce690ddae540f68849448caa67dd92ba108054f20c496ec24742e1a0b42a422cdbdf453b00b949b47f48b8a56972382992e514fa4a4766cc324fbea051701e12320e9ac4344ec20f484acca8c0090270961f6e5e836a0911074b8ca444b6d17197dcf4e95deb1e68204b71913bdc1fb876719793b68839700f4d668ad5c9df1aa57c92a985bd8c488747d1b1b8506090bebb32742d374fc2aa4bf1ace32d8e162540cabb9c6c245c09478db8e4eb8f8157d4e3974c63763f734c3aa081f355ad6feff4759b1b4ab9be4301a109fa8fa40cd1ce982609596e58431dacfd8f06194f764ffcb06da825de417aa81fb6166ab344d66fe586a2ac4d4416809adfff00db1bf14ced4eb89aa9473f8026084a0b08d2daa2bd6462756f43909b1f9793da92e75aa91fbfa91b584dfb9b3ae0829817c0d690dc257ed3124a8e95b9fb758dc1d6d3e7fdac56962741a1af6d2ab654604ef78259b5e4f4ae41f3a0a637927a44647b1d4744c782d0465ca1bdf51d443f63268fecc5db07ad498bb26ab2b94016adc2c714533dda0f423984f087fab5aa7aa86a1274027abd46188a4e70f3ee6215b0fb37dfd372ed45ffa7a74ea2ec31d4f6f632bd5580434f4891c8258eeaba93ab71d9c0df0af84ae89c25a6baf78f83317ce2bfa40c9a407fab3b6a2051b2a470b9559c0a38bd4a45ba1616c5ecb9d441be4536e3c27d49578cb6be2d1ce5327e91ba92cf7d084e62a93ddae944e7448124af3e6f1ff541339787dc74e857a76a2628324017394c569e8c5a1432e471f9c9c12972148c460da2a319486b9b32f71b1aa406861c4d5388a3dcc54a6496d76355e9afbfce42cabaa91204fa6f0c19f450ebd0daf84769a60c6124098f75e74c01b87390e7422aba8014566fb85b91388c0025d5a744afa2d0666bc1d2de578d10293e28b248db9d848ca146eb7cfadd8077dbfc2a4ea9880d80a15885d39ae3ef33a7d90e449ca45bb842ca64177944b2cf05208e4ee5841d02a49028d5001ab4f3b8dc154b73ada722677114d1013a9aaa1bf02d4689ab91c609eb185793956939c8337f41648540be4ed04ee1506b001030869b4538b15e412c94b44bd9d5bcc47618404975b70937e2c263339119a9b10f3ec845ed854e4d5d14403a066b6e4c20195f49079fba3a7bebb8ce08c603dbd1a1244303b18536c43efbaa5eeac49592ff5f44d8ba83d65471ded3943855cd40fb29418cb6776593c4fe794d0c4dd03b31654817921fdad12a1114c80a1c309d90874ab1be495f679e22df922fa86513d65f4fd699209abfeded33886c17db39ccb3433ba5b9103aa1e1e92f574164195716449a892899a41ba27ed21eb9b829ccd745d083c39f2ac2a34c5445e8c27604944348a6afca2d21aff69178b0cb508d894841979bf8937be2a64b9e32edb657344f08bee2bf750b9d7180402e44991b2742879c3e3786dda2333e46b88173c878401aa974cdc7c8152f378736375959fe49c2b5ed5438330801b8b5e96b97447643e8ab7cbaac8195072aabebdcc76d2e4ae9a071ae20ce4cc7d608a9e738c1884f0a95345f5b828725ed9f258369871e4764907cc2227193ab42de7727face594e3f8f5ce5bc79513bb6c7b3d9559a124ef3aafd4be1599dbc6f34577151803140e8ab071f1e14d11da2a62c7f6e40b345c3852db96b7d369ebde6798bb3cc494db29c34397a338712188f3a794489f5465f887693aa10562f2e1792e93a50e44a7e9f37210f3098f2ca7b51b155e0664250b1004010ac3e98260d41d99243b840f994894b65b1263d3cb7f1e4baaffd47fe8cb6892006df1545633a651b7bfc4b50be6ab5dc8a43494d1dfe4f72a2a14da79afebc4433bf7f0d0225e1a9c5814e1c838bce673cc414e685c181efda5f41dcbeb42eff8352b0ab9025ddc6758847ff85f6463007b760868bc649853a7749e48828bf5667415f3d94665346a49943bf8a09b51bf286ad7f80bb2fa90a574280a072023ba803f904e591d098e3c848229a6b0d65d69d7af37325ab0e39954b6aa0f9242885b85cdfe219b941397c461597815791405f5b00f00fad24b8864e239e13818b37ba94ea76225b1dcea74d74871b28024de53319eab84a85fefa4208b3117e4c5865aea6d087965f769347d88012820f85a67e45a36bf608b12c472991cb24618d1fe53bc7b9ced1fb854e66a80fd5a285aa5a2fd45b9a44b1624b0f8ffaa3ef224d0b995b625722da8d4873a274438b32af00abc2e72dd21fc74362881d71568f1ddfa87bd65de139834c48914fd3a04c230deddec60566ca8d4eed97dd38802a602b56286cc342f9b245e495748e04b3e8d8717fef367763144c478afb691b24cf42165209966f4972420eb4f27b92f5a62f2f1e804de14749431388435ee8509eb2ef269c80de818f4c79b7d3e006d7928fd9a5849ee4cdac4370b4ae3270f18b680c1a05d0f38f514c41b9df6b1a413b8c462ba52e0c2299497282c588ab7cc1195c1a5fcc929a6743b7b7036e627362916f777e8985ded642bbbf9b27d8c23822d6085e30fad9674c128e03e39f603da99fd01c20f0d27948c89a11ac1eccf792752e5f8fbb42814c55a4c1c075b10d9ce49e63b2c736804cf7b0e418b6599f2a05e34a621e53a5483da529b29ef7dab7368feef13dfb8d4c058d55dc0acf306022d9e5f182f409433291337cb10a33ea5a06a461c7583e4f58b0dfa447d957f7a5a86fc7bb146e4a7f961d77111786f4a5f0cc58e6a9d94bb081f059989998ff615f53f8d25ff54a48bae2a4f40c7dc94c22e9532b6aba48539a9e5004942b079dc66d5df5faf344e0a225587c8e1c99822141ffab1d6a4b37a379d3dd4b3f1360570e53339fbe4378a29871d6f6d72a3e194b96435b5f4781bceb8311829e8ad89adfa73a97a649f6b19edfa4e94e10df299f95dc1b154b19896f5e34ce8553c804f25dd2c7934af7874f3af52d3cfa30d3";

        private ICertificateProvider certificateProvider;

        [SetUp()]
        public void TestInitialize()
        {
            this.certificateProvider = new CertificateProvider(Mock.Of<ILogger<CertificateProvider>>(), Mock.Of<IDirectory>(), Mock.Of<IAppPathProvider>(), Mock.Of<IDiscoveryServices>());
        }

        [TestCase("A simple test", TestName = "Simple test v1")]
        [TestCase(dataBlock, TestName = "Large data set v1")]
        public void EncryptRoundTripv1(string data)
        {
            EncryptionProvider p = new EncryptionProvider();
            var cert = this.certificateProvider.CreateSelfSignedCert("test");

            string encrypted = p.Encrypt(cert, data, 1);
            string decrypted = p.Decrypt(encrypted, _ => cert);

            Assert.AreEqual(data, decrypted);
        }


        [TestCase("A simple test", TestName = "Simple test v2")]
        [TestCase(dataBlock, TestName = "Large data set v2")]
        public void EncryptRoundTripv2(string data)
        {
            EncryptionProvider p = new EncryptionProvider();
            var cert = this.certificateProvider.CreateSelfSignedCert("test");

            string encrypted = p.Encrypt(cert, data, 2);
            string decrypted = p.Decrypt(encrypted, _ => cert);

            Assert.AreEqual(data, decrypted);
        }
    }
}