﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace DDIntegration.ShiChuangService {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ShiChuangService.WebServiceForDingSoap")]
    public interface WebServiceForDingSoap {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/HelloWorld", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        string HelloWorld();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/HelloWorld", ReplyAction="*")]
        System.Threading.Tasks.Task<string> HelloWorldAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/SelectGroups", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        string SelectGroups(string lineFlag, string userID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/SelectGroups", ReplyAction="*")]
        System.Threading.Tasks.Task<string> SelectGroupsAsync(string lineFlag, string userID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/SelectQuery", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        string SelectQuery(string re_old_code, string datetime);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/SelectQuery", ReplyAction="*")]
        System.Threading.Tasks.Task<string> SelectQueryAsync(string re_old_code, string datetime);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface WebServiceForDingSoapChannel : DDIntegration.ShiChuangService.WebServiceForDingSoap, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class WebServiceForDingSoapClient : System.ServiceModel.ClientBase<DDIntegration.ShiChuangService.WebServiceForDingSoap>, DDIntegration.ShiChuangService.WebServiceForDingSoap {
        
        public WebServiceForDingSoapClient() {
        }
        
        public WebServiceForDingSoapClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public WebServiceForDingSoapClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public WebServiceForDingSoapClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public WebServiceForDingSoapClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public string HelloWorld() {
            return base.Channel.HelloWorld();
        }
        
        public System.Threading.Tasks.Task<string> HelloWorldAsync() {
            return base.Channel.HelloWorldAsync();
        }
        
        public string SelectGroups(string lineFlag, string userID) {
            return base.Channel.SelectGroups(lineFlag, userID);
        }
        
        public System.Threading.Tasks.Task<string> SelectGroupsAsync(string lineFlag, string userID) {
            return base.Channel.SelectGroupsAsync(lineFlag, userID);
        }
        
        public string SelectQuery(string re_old_code, string datetime) {
            return base.Channel.SelectQuery(re_old_code, datetime);
        }
        
        public System.Threading.Tasks.Task<string> SelectQueryAsync(string re_old_code, string datetime) {
            return base.Channel.SelectQueryAsync(re_old_code, datetime);
        }
    }
}