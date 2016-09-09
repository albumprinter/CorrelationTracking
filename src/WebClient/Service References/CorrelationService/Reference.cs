﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebClient.CorrelationService {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="CorrelationService.ICorrelationService")]
    public interface ICorrelationService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ICorrelationService/GetCorrelationId", ReplyAction="http://tempuri.org/ICorrelationService/GetCorrelationIdResponse")]
        System.Guid GetCorrelationId();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ICorrelationService/GetCorrelationId", ReplyAction="http://tempuri.org/ICorrelationService/GetCorrelationIdResponse")]
        System.Threading.Tasks.Task<System.Guid> GetCorrelationIdAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ICorrelationService/ThrowError", ReplyAction="http://tempuri.org/ICorrelationService/ThrowErrorResponse")]
        void ThrowError();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ICorrelationService/ThrowError", ReplyAction="http://tempuri.org/ICorrelationService/ThrowErrorResponse")]
        System.Threading.Tasks.Task ThrowErrorAsync();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface ICorrelationServiceChannel : WebClient.CorrelationService.ICorrelationService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class CorrelationServiceClient : System.ServiceModel.ClientBase<WebClient.CorrelationService.ICorrelationService>, WebClient.CorrelationService.ICorrelationService {
        
        public CorrelationServiceClient() {
        }
        
        public CorrelationServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public CorrelationServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public CorrelationServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public CorrelationServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public System.Guid GetCorrelationId() {
            return base.Channel.GetCorrelationId();
        }
        
        public System.Threading.Tasks.Task<System.Guid> GetCorrelationIdAsync() {
            return base.Channel.GetCorrelationIdAsync();
        }
        
        public void ThrowError() {
            base.Channel.ThrowError();
        }
        
        public System.Threading.Tasks.Task ThrowErrorAsync() {
            return base.Channel.ThrowErrorAsync();
        }
    }
}
