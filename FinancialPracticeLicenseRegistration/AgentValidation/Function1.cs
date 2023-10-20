using System.Net;
using System.Xml;
using System.Xml.Serialization;
using AgentValidation.Models;
//using AgentValidation.Models.FGAgentValidationRequest;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static AgentValidation.Models.NsTXLife;
using agentValidationService;

namespace AgentValidation
{
    public class Function1
    {
        private readonly ILogger _logger;

        public Function1(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Function1>();
        }

        [Function("Function1")]
        public async Task<TXLife> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(requestBody);
            string jsonText = JsonConvert.SerializeXmlNode(doc);
            var alipRequestObject = JsonConvert.DeserializeObject<Models.Root>(jsonText);

            // MAPPING 
            var FGValidationRequestResponse = await CreateMappingRequest(alipRequestObject);


            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.WriteString("Welcome to Azure Functions!");

            return FGValidationRequestResponse;

            // 1. Sample successful request
            // 2. I will create custom c# class model for this
            // 3. I will extract the fields from request and then I will map this to out going req(f&g)
            // 4. I will convert f&g model into xml
            // 5. Finally we will call the f&g webserive using basic authentication.

        }

        private Task<TXLife> CreateMappingRequest(Models.Root alipRequest)
        {
            EAppAgentValidationClient eAppAgentValidationClient = new EAppAgentValidationClient();
            TXLife tXLife = new TXLife();
            TXLifeRequest tXLifeRequest = new TXLifeRequest();
            TXLife_Type tXLife_Type = new TXLife_Type();
            TXLifeRequest_Type tXLifeRequest_Type = new TXLifeRequest_Type();

            tXLifeRequest.TransRefGUID = alipRequest.nsTXLife.nsTXLifeRequest.nsTransRefGUID;
            TransType transType = new TransType();
            transType.Value = alipRequest.nsTXLife.nsTXLifeRequest.nsTransType.text;
            transType.tc = alipRequest.nsTXLife.nsTXLifeRequest.nsTransType.tc;
            tXLifeRequest.TransType = transType;

            TransSubType transSubType = new TransSubType();
            TRANS_SUBTYPE_CODES tRANS_SUBTYPE_CODES = new TRANS_SUBTYPE_CODES();
            transSubType.Value = alipRequest.nsTXLife.nsTXLifeRequest.nsTransSubType.text;
            transSubType.tc = alipRequest.nsTXLife.nsTXLifeRequest.nsTransSubType.tc;
            tXLifeRequest.TransSubType = transSubType;
            tXLifeRequest.TransExeDate = Convert.ToDateTime("2018-03-15");
            tXLifeRequest.TransExeTime = Convert.ToDateTime("05:18:35");

            OLifE oLifE = new OLifE();
            OLifE_Type oLifE_Type = new OLifE_Type();
            SourceInfo sourceInfo = new SourceInfo();
            ApplicationInfo applicationInfo = new ApplicationInfo();
            OLI_LU_STATE oLI_LU_STATE = new OLI_LU_STATE();
            oLI_LU_STATE.tc =
                alipRequest.nsTXLife.nsTXLifeRequest.nsOLifE.nsHolding.nsPolicy
                .nsApplicationInfo.nsApplicationJurisdiction.tc;
            ApplicationJurisdiction applicationJurisdiction = new ApplicationJurisdiction();
            applicationJurisdiction.tc = alipRequest.nsTXLife.nsTXLifeRequest.nsOLifE.nsHolding.nsPolicy
                .nsApplicationInfo.nsApplicationJurisdiction.tc;
            applicationInfo.ApplicationJurisdiction = applicationJurisdiction;
            applicationInfo.SignedDate =Convert.ToDateTime(alipRequest.nsTXLife.nsTXLifeRequest.nsOLifE.nsHolding.nsPolicy
                .nsApplicationInfo.nsSignedDate);
            applicationInfo.ApplicationCollectionDate = Convert.ToDateTime(alipRequest.nsTXLife.nsTXLifeRequest.nsOLifE.nsHolding.nsPolicy
                .nsApplicationInfo.nsApplicationCollectionDate);

            Policy policy = new Policy();
            LineOfBusiness lineOfBusiness = new LineOfBusiness();
            lineOfBusiness.tc = "11";
            lineOfBusiness.Value = "Annuity";
            policy.LineOfBusiness = lineOfBusiness;
            ProductType productType = new ProductType()
            {
                tc = "11",
                Value = "Indexed Annuity - not otherwise specified"
            };
            policy.ProductType = productType;
            policy.CusipNum = "31578L142";
            Payout payout = new Payout();
            payout.PayoutAmt = 10000;
            QualPlanType qualPlanType = new QualPlanType();
            qualPlanType.tc = "1";
            qualPlanType.Value = "Non-Qualified";
            Annuity annuity = new Annuity();
            annuity.QualPlanType = qualPlanType;
            annuity.Payout = new Payout[1];
            annuity.Payout[0]= payout;
            policy.Annuity = annuity;


            policy.ProductCode = alipRequest.nsTXLife.nsTXLifeRequest.nsOLifE.nsHolding
                .nsPolicy
                .nsProductCode;
            PolicyStatus policyStatus = new PolicyStatus();
            policyStatus.Value = alipRequest.nsTXLife.nsTXLifeRequest.nsOLifE.nsHolding
                .nsPolicy.nsPolicyStatus.text;
            policyStatus.tc = alipRequest.nsTXLife.nsTXLifeRequest.nsOLifE.nsHolding
                .nsPolicy.nsPolicyStatus.tc;
            policy.PolicyStatus = policyStatus;
            policy.ApplicationInfo = applicationInfo;

            Holding holding = new Holding();
            holding.Policy = policy;
            holding.id = alipRequest.nsTXLife.nsTXLifeRequest.nsOLifE.nsHolding.id;
            HoldingTypeCode holdingTypeCode = new HoldingTypeCode();

            holdingTypeCode.Value = alipRequest.nsTXLife.nsTXLifeRequest
                                            .nsOLifE.nsHolding.nsHoldingTypeCode.text;
            holdingTypeCode.tc = alipRequest.nsTXLife.nsTXLifeRequest
                                            .nsOLifE.nsHolding.nsHoldingTypeCode.tc;
            holding.HoldingTypeCode = holdingTypeCode;
            oLifE.Holding = new Holding[1];
            oLifE.Holding[0] = holding;

            CarrierAppointment carrierAppointment = new CarrierAppointment();
            DistributionChannelInfo distributionChannelInfo = new DistributionChannelInfo();
            DistributionChannel distributionChannel = new DistributionChannel();
            distributionChannel.tc = alipRequest.nsTXLife.nsTXLifeRequest
                                            .nsOLifE.nsParty[0].nsProducer.nsCarrierAppointment.
                                            nsDistributionChannelInfo.nsDistributionChannel.tc;
            distributionChannelInfo.DistributionChannel = distributionChannel;
            carrierAppointment.DistributionChannelInfo = new DistributionChannelInfo[1];
            carrierAppointment.DistributionChannelInfo[0] = distributionChannelInfo;
            carrierAppointment.id = "CarrierAppointment_b2db88d2-e05c-48f0-9ab6-d8dbe791ca1c";
            carrierAppointment.CompanyProducerID = "000454223";
            carrierAppointment.CarrierCode = "FGL";

            Producer producer = new Producer();
            producer.CarrierAppointment  = new CarrierAppointment[1];
            producer.CarrierAppointment[0] = carrierAppointment;
            Party party = new Party();
            party.id = "Party_f0eb29a3-356b-4c31-8b4a-aed0666177dc";
            PartyTypeCode partyTypeCode = new PartyTypeCode();
            partyTypeCode.tc = "1";
            partyTypeCode.Value = "Person";
            party.PartyTypeCode = partyTypeCode;
            party.Producer = producer;

            Relation relation = new Relation();
            relation.id = "Relation_ee90f802-c50b-4dfd-b0c9-d2f3d1a8c54a";
            relation.OriginatingObjectID = "Holding_4c739778-5bc0-4391-9ca2-d4baaac5a29f";
            relation.RelatedObjectID = "Party_f0eb29a3-356b-4c31-8b4a-aed0666177dc";
            OriginatingObjectType originatingObjectType = new OriginatingObjectType();
            originatingObjectType.tc = "4";
            originatingObjectType.Value = "Holding";
            relation.OriginatingObjectType = originatingObjectType;
            RelatedObjectType relatedObjectType = new RelatedObjectType();
            relatedObjectType.tc = "37";
            relatedObjectType.Value = "OLI_PARTY";
            relation.RelatedObjectType = relatedObjectType;
            RelationRoleCode relationRoleCode = new RelationRoleCode();
            relationRoleCode.tc = "37";
            relationRoleCode.Value = "OLI_REL_PRIMAGENT";
            relation.RelationRoleCode = relationRoleCode;
            relation.RelatedRefID = "000454223";
            RelatedRefIDType relatedRefIDType = new RelatedRefIDType();
            relatedRefIDType.tc = "33";
            relatedRefIDType.Value = "Carrier Assigned Producer Identification Number";
            relation.RelatedRefIDType = relatedRefIDType;

            sourceInfo.SourceInfoName = "FireLightEApp";
            oLifE.SourceInfo = sourceInfo;
            oLifE.Holding = new Holding[1];
            oLifE.Holding[0] = holding;
            oLifE.Party = new Party[1];
            oLifE.Party[0] = party;
            oLifE.Relation = new Relation[1];
            oLifE.Relation[0] = relation;
            tXLifeRequest.OLifE = oLifE;
            tXLife.TXLifeRequest =new TXLifeRequest[1];
            tXLife.TXLifeRequest[0] = tXLifeRequest;
            var response = eAppAgentValidationClient.GetEAppAgentValidationDetailsAsync(tXLife);

            return response;
        }
    }
}
