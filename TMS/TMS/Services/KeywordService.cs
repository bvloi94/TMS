using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TMS.DAL;
using TMS.Models;

namespace TMS.Services
{
    public class KeywordService
    {
        private readonly UnitOfWork _unitOfWork;

        public KeywordService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Keyword> GetAll()
        {
            return _unitOfWork.KeywordRepository.Get();
        }

        public ICollection<TicketKeyword> GetTicketKeywordsForCreate(string keywords)
        {
            List<TicketKeyword> result = new List<TicketKeyword>();
            if (keywords != null)
            {
                string[] keywordArr = keywords.Split(',');
                foreach (string keyword in keywordArr)
                {
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        string temp = keyword.Trim();
                        Keyword keywordObj = GetKeywordByKeyword(temp);
                        TicketKeyword ticketKeyword;
                        if (keywordObj != null)
                        {
                            ticketKeyword = new TicketKeyword
                            {
                                KeywordID = keywordObj.ID
                            };
                            result.Add(ticketKeyword);
                        }
                        else
                        {
                            Keyword newKeywordObj = new Keyword
                            {
                                Name = temp
                            };
                            bool addKeyword = AddKeyword(newKeywordObj);
                            if (addKeyword)
                            {
                                ticketKeyword = new TicketKeyword
                                {
                                    KeywordID = newKeywordObj.ID
                                };
                                result.Add(ticketKeyword);
                            }
                        }
                    }
                }
            }

            return result;
        }

        private Keyword GetKeywordByKeyword(string keyword)
        {
            return _unitOfWork.KeywordRepository.Get(m => m.Name.ToLower().Equals(keyword.ToLower())).FirstOrDefault();
        }

        private bool AddKeyword(Keyword keyword)
        {
            keyword.Name = keyword.Name.ToLower();
            _unitOfWork.KeywordRepository.Insert(keyword);
            return _unitOfWork.Commit();
        }

        public string GetTicketKeywordForDisplay(int ticketId)
        {
            ICollection<TicketKeyword> ticketKeywords = _unitOfWork.TicketRepository.GetByID(ticketId).TicketKeywords;
            string result = string.Empty;
            if (ticketKeywords != null)
            {
                string delimeter = string.Empty;
                foreach (TicketKeyword ticketKeyword in ticketKeywords)
                {
                    result += delimeter + ticketKeyword.Keyword.Name;
                    delimeter = ",";
                }
            }
            return result;
        }

        public string GetSolutionKeywordForDisplay(int solutionId)
        {
            ICollection<SolutionKeyword> solutionKeywords = _unitOfWork.SolutionRepository.GetByID(solutionId).SolutionKeywords;
            string result = string.Empty;
            if (solutionKeywords != null)
            {
                string delimeter = string.Empty;
                foreach (SolutionKeyword solutionKeyword in solutionKeywords)
                {
                    result += delimeter + solutionKeyword.Keyword.Name;
                    delimeter = ",";
                }
            }
            return result;
        }

        public ICollection<TicketKeyword> GetTicketKeywordsForEdit(string keywords, int ticketId)
        {
            List<TicketKeyword> result = new List<TicketKeyword>();
            if (keywords != null)
            {
                string[] keywordArr = keywords.Split(',');
                foreach (string keyword in keywordArr)
                {
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        string temp = keyword.Trim();
                        Keyword keywordObj = GetKeywordByKeyword(temp);
                        TicketKeyword ticketKeyword;
                        if (keywordObj != null)
                        {
                            ticketKeyword = new TicketKeyword
                            {
                                KeywordID = keywordObj.ID,
                                TicketID = ticketId
                            };
                            result.Add(ticketKeyword);
                        }
                        else
                        {
                            Keyword newKeywordObj = new Keyword
                            {
                                Name = temp
                            };
                            bool addKeyword = AddKeyword(newKeywordObj);
                            if (addKeyword)
                            {
                                ticketKeyword = new TicketKeyword
                                {
                                    KeywordID = newKeywordObj.ID,
                                    TicketID = ticketId
                                };
                                result.Add(ticketKeyword);
                            }
                        }
                    }
                }
            }

            return result;
        }

        public ICollection<SolutionKeyword> GetSolutionKeywordsForCreate(string keywords)
        {
            List<SolutionKeyword> result = new List<SolutionKeyword>();
            if (keywords != null)
            {
                string[] keywordArr = keywords.Split(',');
                foreach (string keyword in keywordArr)
                {
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        string temp = keyword.Trim();
                        Keyword keywordObj = GetKeywordByKeyword(temp);
                        SolutionKeyword solutionKeyword;
                        if (keywordObj != null)
                        {
                            solutionKeyword = new SolutionKeyword
                            {
                                KeywordID = keywordObj.ID
                            };
                            result.Add(solutionKeyword);
                        }
                        else
                        {
                            Keyword newKeywordObj = new Keyword
                            {
                                Name = temp
                            };
                            bool addKeyword = AddKeyword(newKeywordObj);
                            if (addKeyword)
                            {
                                solutionKeyword = new SolutionKeyword
                                {
                                    KeywordID = newKeywordObj.ID
                                };
                                result.Add(solutionKeyword);
                            }
                        }
                    }
                }
            }

            return result;
        }

        public ICollection<SolutionKeyword> GetSolutionKeywordsForEdit(string keywords, int solutionId)
        {
            List<SolutionKeyword> result = new List<SolutionKeyword>();
            if (keywords != null)
            {
                string[] keywordArr = keywords.Split(',');
                foreach (string keyword in keywordArr)
                {
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        string temp = keyword.Trim();
                        Keyword keywordObj = GetKeywordByKeyword(temp);
                        SolutionKeyword solutionKeyword;
                        if (keywordObj != null)
                        {
                            solutionKeyword = new SolutionKeyword
                            {
                                KeywordID = keywordObj.ID,
                                SolutionID = solutionId
                            };
                            result.Add(solutionKeyword);
                        }
                        else
                        {
                            Keyword newKeywordObj = new Keyword
                            {
                                Name = temp
                            };
                            bool addKeyword = AddKeyword(newKeywordObj);
                            if (addKeyword)
                            {
                                solutionKeyword = new SolutionKeyword
                                {
                                    KeywordID = newKeywordObj.ID,
                                    SolutionID = solutionId
                                };
                                result.Add(solutionKeyword);
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}