namespace CBT3_Mediator;
public interface IRequest :IBaseRequest {}

public interface IBaseRequest { }

public interface IRequest<in TResponse> : IBaseRequest { }
